using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Burrows.Logging;
using Burrows.PublisherConfirms.BackingStores;

namespace Burrows.PublisherConfirms
{
    public class Publisher : IDisposable, IPublisher
    {
        private readonly ConcurrentQueue<ConfirmableMessage> _unpublishedMessageBuffer = new ConcurrentQueue<ConfirmableMessage>();

        private readonly object _failuresLock = new object();
        private volatile bool _publicationEnabled = true;

// ReSharper disable NotAccessedField.Local
        private readonly Timer _checkOfflineTasksTimer;
// ReSharper restore NotAccessedField.Local

        private int _successiveFailures;
        private long _lastMessageBufferStoreTimestamp;
        private long _lastPublishRetryTimestamp;
        private int _processingBufferedMessages;
        private int _retryingPublish;
        private bool _disposed;

        private readonly IConfirmer _confirmer;
        private readonly IUnconfirmedMessageRepository _messageRepository;
        private readonly PublishSettings _publishSettings;
        private readonly Lazy<IServiceBus> _serviceBus;
        private static readonly ILog _log = Logger.Get<Publisher>();

        public Publisher(Lazy<IServiceBus> serviceBus, PublishSettings publishSettings, IConfirmer confirmer, 
            IUnconfirmedMessageRepositoryFactory unconfirmedMessageRepositoryFactory)
        {
            _publishSettings = publishSettings;
            _publishSettings.Validate();

            _confirmer = confirmer;
            _messageRepository = unconfirmedMessageRepositoryFactory.Create();
            _serviceBus = serviceBus;

            _confirmer.PublicationFailed += OnPublicationFailed;
            _confirmer.PublicationSucceeded += OnPublicationSucceeded;

            _lastMessageBufferStoreTimestamp = DateTime.UtcNow.Ticks;
            _lastPublishRetryTimestamp = DateTime.UtcNow.Ticks;
            _checkOfflineTasksTimer = new Timer(CheckOfflineTasks, null, _publishSettings.TimerCheckInterval, _publishSettings.TimerCheckInterval);

            RepublishStoredMessages().Wait();
        }

        /// <summary>
        /// Performs message publication.
        /// </summary>
        /// <typeparam name="T">Type to publish</typeparam>
        /// <param name="message">Message to publish</param>
        /// <param name="forcePublish">Force a publish even if publishing is not currently enabled</param>
        public void Publish<T>(T message, bool forcePublish = false)
        {
            var confirmableMessage = new ConfirmableMessage(message, typeof(T));

            if (PublishImmediately || forcePublish)
            {
                try
                {
                    _confirmer.RecordPublicationAttempt(confirmableMessage);
                    _serviceBus.Value.Publish(message, context => context.SetHeader("ClientMessageId", confirmableMessage.Id));
                }
                catch (Exception ex)
                {
                    _confirmer.RecordPublicationFailure(new[]{confirmableMessage.Id});
                    _log.Error("An error occurred while publishing to MassTransit", ex);
                }
            }
            else
            {
                _unpublishedMessageBuffer.Enqueue(confirmableMessage);
            }
        }

        /// <summary>
        /// Called by the confirmer when publication fails.  
        /// Adds unconfirmed messages to the unpublished buffer and shuts down publication after enough successive failures.
        /// </summary>
        private void OnPublicationFailed(IEnumerable<ConfirmableMessage> messages)
        {
            foreach (var message in messages)
            {
                _unpublishedMessageBuffer.Enqueue(message);
            }
            
            Interlocked.Increment(ref _successiveFailures);

            if (_publicationEnabled && _successiveFailures >= _publishSettings.MaxSuccessiveFailures)
            {
                lock (_failuresLock)
                {
                    _publicationEnabled = false;
                }
            }
        }

        /// <summary>
        /// Called by the confirmer when publication succeeds.  
        /// Typically used to publish stored messages when recovering from a rabbit failure state.
        /// </summary>
        private void OnPublicationSucceeded(IEnumerable<ConfirmableMessage> unconfirmedMessages)
        {
            Interlocked.Exchange(ref _successiveFailures, 0);

            if (!_publicationEnabled)
            {
                lock (_failuresLock)
                {
                    _publicationEnabled = _successiveFailures < _publishSettings.MaxSuccessiveFailures;
                    if (_publicationEnabled)
                    {
                        //Good to go again, republish stored messages.
                        RepublishStoredMessages().Wait();
                    }
                }
            }
        }

        /// <summary>
        /// Is publishing currently enabled or has it been shut down due to errors.
        /// </summary>
        public bool PublicationEnabled
        {
            get { return _publicationEnabled; }
        }

        private bool PublishImmediately
        {
            get { return _publicationEnabled && !Convert.ToBoolean(_retryingPublish); }
        }

        /// <summary>
        /// The timer kicks this off to see if we need to retry stored messages or process buffered messages.
        /// </summary>
        /// <param name="state"></param>
        private void CheckOfflineTasks(object state)
        {
            var checkTime = DateTime.UtcNow.Ticks;

            if ((checkTime - _lastPublishRetryTimestamp) / TimeSpan.TicksPerMillisecond >= _publishSettings.PublishRetryInterval)
            {
                RepublishOneStoredMessage().Wait();
            }

            checkTime = DateTime.UtcNow.Ticks;
            if ((checkTime - _lastMessageBufferStoreTimestamp) / TimeSpan.TicksPerMillisecond >= _publishSettings.ProcessBufferedMessagesInterval)
            {
                ProcessBufferedMessages();
            }
        }

        /// <summary>
        /// Run every now and then to clear the buffer if it has messages trapped, note, this is a failsafe that should rarely be needed.
        /// </summary>
        private void ProcessBufferedMessages()
        {
            if (Convert.ToBoolean(_retryingPublish) || Convert.ToBoolean(Interlocked.CompareExchange(ref _processingBufferedMessages, 1, 0)))
                return;

            if (_publicationEnabled)
            {
                try
                {
                    PublishFromBuffer();
                }
                finally
                {
                    Interlocked.Exchange(ref _lastMessageBufferStoreTimestamp, DateTime.UtcNow.Ticks);
                    Interlocked.Exchange(ref _processingBufferedMessages, 0);
                }
            }
            else
            {
                Task.Factory.StartNew(() =>
                                          {
                                              try
                                              {
                                                  _messageRepository.StoreMessages(_unpublishedMessageBuffer, _publishSettings .PublisherId);
                                              }
                                              finally
                                              {
                                                  Interlocked.Exchange(ref _lastMessageBufferStoreTimestamp, DateTime.UtcNow.Ticks);
                                                  Interlocked.Exchange(ref _processingBufferedMessages, 0);
                                              }
                                          });
            }
        }


        /// <summary>
        /// Used when attempting to republish all stored messages.
        /// </summary>
        private async Task RepublishStoredMessages()
        {
            if (!_publicationEnabled)
                return;

            Interlocked.Exchange(ref _retryingPublish, 1);

            await Task.Factory.StartNew(async () =>
            {
                try
                {
                    //Retry messages in the DB first
                    while (_publicationEnabled)
                    {
                        IList<ConfirmableMessage> storedMessages = 
                            await _messageRepository.GetAndDeleteMessages(_publishSettings.PublisherId, _publishSettings.GetStoredMessagesBatchSize);

                        if (storedMessages.Count <= 0)
                            break;

                        foreach (var storedMessage in storedMessages)
                        {
                            Publish(storedMessage);
                        }
                    }

                    PublishFromBuffer();
                }
                finally
                {
                    Interlocked.Exchange(ref _retryingPublish, 0);
                }
            });
        }

        /// <summary>
        /// Used when attempting to see whether or not publishing can be restarted
        /// </summary>
        private async Task RepublishOneStoredMessage()
        {
            if (_publicationEnabled || Convert.ToBoolean(_processingBufferedMessages) || Convert.ToBoolean(Interlocked.CompareExchange(ref _retryingPublish, 1, 0)))
                return;
            
            await Task.Factory.StartNew(async () => 
            {
                try
                {
                    ConfirmableMessage message = (await _messageRepository.GetAndDeleteMessages(_publishSettings.PublisherId, 1)).FirstOrDefault();

                    if (message != null)
                    {
                        Publish(message, true);
                    }
                }
                finally
                {
                    Interlocked.Exchange(ref _lastPublishRetryTimestamp, DateTime.UtcNow.Ticks);
                    Interlocked.Exchange(ref _retryingPublish, 0);
                }
            });
        }

        /// <summary>
        /// Publish all messages from the buffer if publication is turned on.
        /// </summary>
        private void PublishFromBuffer()
        {
            ConfirmableMessage message;
            while (_publicationEnabled && _unpublishedMessageBuffer.TryDequeue(out message))
            {
                Publish(message, true);
            }
        }

        public void Dispose()
        {
            if (_disposed)
            {
                if (_confirmer != null)
                {
                    _confirmer.PublicationFailed -= OnPublicationFailed;
                    _confirmer.PublicationSucceeded -= OnPublicationSucceeded;
                }
                _disposed = true;

                GC.SuppressFinalize(this);
            }
        }

    }
}
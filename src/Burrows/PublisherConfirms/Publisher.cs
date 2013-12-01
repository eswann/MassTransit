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
        private readonly ConcurrentQueue<ConfirmableMessage> _messageBuffer = new ConcurrentQueue<ConfirmableMessage>();

        private readonly object _failuresLock = new object();
        private volatile bool _publicationEnabled = true;
        private readonly Timer _checkOfflineTasksTimer;

        private int _successiveFailures;
        private long _lastMessageBufferStoreTimestamp;
        private long _lastPublishRetryTimestamp;
        private int _processingBufferedMessages;
        private int _retryingPublish;
        private bool _disposed;

        private readonly IConfirmer _confirmer;
        private readonly IUnconfirmedMessageRepository _unconfirmedMessageRepository;
        private readonly PublishSettings _publishSettings;
        private readonly Lazy<IServiceBus> _serviceBus;
        private static readonly ILog _log = Logger.Get<Publisher>();

        public Publisher(Lazy<IServiceBus> serviceBus, PublishSettings publishSettings, IConfirmer confirmer, 
            IUnconfirmedMessageRepositoryFactory unconfirmedMessageRepositoryFactory)
        {
            _publishSettings = publishSettings;
            _publishSettings.Validate();

            _confirmer = confirmer;
            _unconfirmedMessageRepository = unconfirmedMessageRepositoryFactory.Create();
            _serviceBus = serviceBus;

            _confirmer.PublicationFailed += OnPublicationFailed;
            _confirmer.PublicationSucceeded += OnPublicationSucceeded;

            _lastMessageBufferStoreTimestamp = DateTime.UtcNow.Ticks;
            _lastPublishRetryTimestamp = DateTime.UtcNow.Ticks;
            _checkOfflineTasksTimer = new Timer(CheckOfflineTasks, null, _publishSettings.TimerCheckInterval, _publishSettings.TimerCheckInterval);

            RepublishStoredMessages().Wait();
        }

        public void Publish<T>(T message, bool forcePublish = false)
        {
            var confirmableMessage = new ConfirmableMessage
            {
                Id = Guid.NewGuid().ToString("N"),
                Message = message,
                Type = typeof(T)
            };

            if (PublicationEnabled || forcePublish)
            {
                try
                {
                    _serviceBus.Value.Publish(message, context => context.SetHeader("ClientMessageId", confirmableMessage.Id));
                }
                catch (Exception ex)
                {
                    _messageBuffer.Enqueue(confirmableMessage);
                    OnPublicationFailed();
                    _log.Error("An error occurred while publishing to MassTransit", ex);
                }
            }
            else
            {
                _messageBuffer.Enqueue(confirmableMessage);
            }
        }

        private void OnPublicationFailed()
        {
            Interlocked.Increment(ref _successiveFailures);

            if (_publicationEnabled && _successiveFailures >= _publishSettings.MaxSuccessiveFailures)
            {
                lock (_failuresLock)
                {
                    _publicationEnabled = false;
                    PersistUnconfirmedMessages();
                }
            }
        }

        private void OnPublicationSucceeded()
        {
            Interlocked.Exchange(ref _successiveFailures, 0);

            if (!_publicationEnabled)
            {
                lock (_failuresLock)
                {
                    _publicationEnabled = _successiveFailures < _publishSettings.MaxSuccessiveFailures;
                    if (_publicationEnabled)
                    {
                        RepublishStoredMessages().Wait();
                    }
                }
            }
        }

        public bool PublicationEnabled
        {
            get { return _publicationEnabled; }
        }

        private void PersistUnconfirmedMessages()
        {
            Task.Factory.StartNew(() =>
            {
                var unconfirmedMessages = _confirmer.GetUnconfirmedMessages();
                _unconfirmedMessageRepository.StoreMessages(unconfirmedMessages, _publishSettings.PublisherId);
                _confirmer.RemoveUnconfirmedMessages(unconfirmedMessages);
            });
        }

        private void CheckOfflineTasks(object state)
        {
            var checkTime = DateTime.UtcNow.Ticks;

            if ((checkTime - _lastPublishRetryTimestamp) / TimeSpan.TicksPerMillisecond >=
                _publishSettings.PublishRetryInterval)
            {
                RepublishStoredMessage().Wait();
            }

            checkTime = DateTime.UtcNow.Ticks;
            if ((checkTime - _lastMessageBufferStoreTimestamp) / TimeSpan.TicksPerMillisecond >=
                _publishSettings.ProcessBufferedMessagesInterval)
            {
                ProcessBufferedMessages();
            }
        }

        private void ProcessBufferedMessages()
        {
            if (Convert.ToBoolean(_retryingPublish) ||
                Convert.ToBoolean(Interlocked.CompareExchange(ref _processingBufferedMessages, 1, 0)))
                return;

            if (_publicationEnabled)
            {
                try
                {
                    RepublishFromBuffer();
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
                                                  _unconfirmedMessageRepository.StoreMessages(_messageBuffer, _publishSettings .PublisherId);
                                              }
                                              finally
                                              {
                                                  Interlocked.Exchange(ref _lastMessageBufferStoreTimestamp, DateTime.UtcNow.Ticks);
                                                  Interlocked.Exchange(ref _processingBufferedMessages, 0);
                                              }
                                          });
            }

        }

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
                        IEnumerable<ConfirmableMessage> storedMessages =
                            await _unconfirmedMessageRepository.GetAndDeleteMessages(_publishSettings.PublisherId,
                                                                               _publishSettings.GetStoredMessagesBatchSize);

                        if (!storedMessages.Any())
                            break;

                        foreach (var storedMessage in storedMessages)
                        {
                            Publish(storedMessage);
                        }
                    }

                    RepublishFromBuffer();
                }
                finally
                {
                    Interlocked.Exchange(ref _retryingPublish, 0);
                }
            });
        }

        private async Task RepublishStoredMessage()
        {
            if (_publicationEnabled || Convert.ToBoolean(_processingBufferedMessages) || Convert.ToBoolean(Interlocked.CompareExchange(ref _retryingPublish, 1, 0)))
                return;
            
            await Task.Factory.StartNew(async () => 
            {
                try
                {
                    ConfirmableMessage message = (await _unconfirmedMessageRepository.GetAndDeleteMessages(_publishSettings.PublisherId, 1)).FirstOrDefault();

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

        private void RepublishFromBuffer()
        {
            ConfirmableMessage message;
            while (_publicationEnabled && _messageBuffer.TryDequeue(out message))
            {
                Publish(message);
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
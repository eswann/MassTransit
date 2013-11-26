namespace MassTransit.Transports.RabbitMq.Publish
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class Confirmer : IConfirmer
    {
        static readonly ConcurrentDictionary<Guid, UnconfirmedMessage> _publishedMessages =
            new ConcurrentDictionary<Guid, UnconfirmedMessage>();

        static readonly ConcurrentDictionary<ulong, UnconfirmedMessage> _unconfirmedMessages =
            new ConcurrentDictionary<ulong, UnconfirmedMessage>();

        public event Action PublicationFailed;
        public event Action PublicationSucceeded;

        public void RecordMessagePublication(IMessage message)
        {
            _publishedMessages.TryAdd(message.MessageId, new UnconfirmedMessage{MessageId = message.MessageId, Message = message});
        }

        public void RecordRabbitPublication(ulong sequenceNumber, string clientMessageId)
        {
            UnconfirmedMessage message;
            if (_publishedMessages.TryRemove(Guid.Parse(clientMessageId), out message))
            {
                message.SequenceNumber = sequenceNumber;
                _unconfirmedMessages.TryAdd(sequenceNumber, message);
            }
        }

        public void RecordPublicationFailure(ulong confirmableMessageId, bool isUpperBound)
        {
            if (PublicationFailed != null)
                PublicationFailed();
        }

        public void ConfirmPublication(ulong sequenceNumber, bool isUpperBound)
        {
            UnconfirmedMessage confirmedMessage;
            if (isUpperBound)
            {
                IEnumerable<ulong> confirmKeysToRemove = _unconfirmedMessages.Keys.Where(x => x <= sequenceNumber);

                foreach (var confirmKey in confirmKeysToRemove)
                {
                    _unconfirmedMessages.TryRemove(confirmKey, out confirmedMessage);
                }
            }
            else
            {
                _unconfirmedMessages.TryRemove(sequenceNumber, out confirmedMessage);
            }

            if (PublicationSucceeded != null)
                PublicationSucceeded();
        }

        public void RemoveUnconfirmedMessages(IEnumerable<UnconfirmedMessage> messages)
        {
            UnconfirmedMessage message;
            foreach (var unconfirmedMessage in messages)
            {
                if (unconfirmedMessage.SequenceNumber > 0)
                {
                    _unconfirmedMessages.TryRemove(unconfirmedMessage.SequenceNumber, out message);
                }
                else
                {
                    _publishedMessages.TryRemove(unconfirmedMessage.MessageId, out message);
                }
            }
        }

        public IEnumerable<UnconfirmedMessage> GetUnconfirmedMessages()
        {
            return _unconfirmedMessages.Values.Union(_publishedMessages.Values);
        }
    }
}
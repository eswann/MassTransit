using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Burrows.PublisherConfirms
{
    public class Confirmer : IConfirmer
    {
        private readonly ConcurrentDictionary<string, ConfirmableMessage> _unconfirmedMessages =
            new ConcurrentDictionary<string, ConfirmableMessage>();

        public event Action<IEnumerable<ConfirmableMessage>> PublicationFailed;
        public event Action<IEnumerable<ConfirmableMessage>> PublicationSucceeded;

        public void RecordPublicationAttempt(ConfirmableMessage message)
        {
            _unconfirmedMessages.TryAdd(message.Id, message);
        }

        public void RecordPublicationSuccess(IEnumerable<string> messageIds)
        {
            var confirmableMessages = RemoveMessages(messageIds);

            if (PublicationSucceeded != null)
                PublicationSucceeded(confirmableMessages);
        }

        public void RecordPublicationFailure(IEnumerable<string> messageIds)
        {
            var messages = RemoveMessages(messageIds);

            if (PublicationFailed != null)
                PublicationFailed(messages);
        }

        public void ClearMessages()
        {
            _unconfirmedMessages.Clear();
        }

        public IEnumerable<ConfirmableMessage> RemoveMessages(IEnumerable<string> messageIds)
        {
            var removedMessages = new List<ConfirmableMessage>();

            foreach (var messageId in messageIds)
            {
                ConfirmableMessage message;
                _unconfirmedMessages.TryRemove(messageId, out message);
                if (message != null)
                {
                    removedMessages.Add(message);
                }
            }

            return removedMessages;
        }

        public ICollection<ConfirmableMessage> GetMessages()
        {
            return _unconfirmedMessages.Values;
        }
    }
}
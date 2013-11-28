using System;
using System.Collections.Generic;

namespace Burrows.BackedPublisher
{
    public interface IConfirmer
    {
        event Action PublicationFailed;
        event Action PublicationSucceeded;
        void RecordMessagePublication(Object message);
        void RecordRabbitPublication(ulong sequenceNumber, string clientMessageId);
        void RecordPublicationFailure(ulong confirmableMessageId, bool isUpperBound);
        void ConfirmPublication(ulong sequenceNumber, bool isUpperBound);
        void RemoveUnconfirmedMessages(IEnumerable<UnconfirmedMessage> messages);
        IEnumerable<UnconfirmedMessage> GetUnconfirmedMessages();
    }
}
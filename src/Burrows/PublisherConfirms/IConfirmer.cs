using System;
using System.Collections.Generic;

namespace Burrows.PublisherConfirms
{
    public interface IConfirmer
    {
        event Action<IEnumerable<ConfirmableMessage>> PublicationFailed;
        event Action<IEnumerable<ConfirmableMessage>> PublicationSucceeded;
        void RecordPublicationAttempt(ConfirmableMessage message);
        void RecordPublicationFailure(IEnumerable<string> messageIds);
        void RecordPublicationSuccess(IEnumerable<string> messageIds);
        void ClearMessages();
        ICollection<ConfirmableMessage> GetMessages();
    }
}
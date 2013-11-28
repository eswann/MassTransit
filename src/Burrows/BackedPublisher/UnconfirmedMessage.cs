using System;

namespace Burrows.BackedPublisher
{
    public class UnconfirmedMessage
    {
        public Guid MessageId { get; set; }

        public ulong SequenceNumber { get; set; }

        public object Message { get; set; }
    }
}
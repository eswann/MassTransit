namespace MassTransit.Transports.RabbitMq.Publish
{
    using System;

    public class UnconfirmedMessage
    {
        public Guid MessageId { get; set; }

        public ulong SequenceNumber { get; set; }

        public IMessage Message { get; set; }
    }
}
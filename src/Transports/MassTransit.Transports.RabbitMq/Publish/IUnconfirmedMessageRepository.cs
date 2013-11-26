namespace MassTransit.Transports.RabbitMq.Publish
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    public interface IUnconfirmedMessageRepository
    {
        IEnumerable<IMessage> GetAndDeleteMessages(string publisherId, int pageSize);

        void StoreMessages(ConcurrentQueue<IMessage> messages, string publisherId);

        void StoreMessages(IEnumerable<IMessage> messages, string publisherId);
    }
}
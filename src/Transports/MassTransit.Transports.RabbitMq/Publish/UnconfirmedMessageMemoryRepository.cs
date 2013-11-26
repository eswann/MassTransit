namespace MassTransit.Transports.RabbitMq.Publish
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;

    public class UnconfirmedMessageMemoryRepository : IUnconfirmedMessageRepository
    {
        private readonly List<IMessage> _messageStore = new List<IMessage>();
        private static readonly object _syncLock = new object();

        public IEnumerable<IMessage> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            IEnumerable<IMessage> results;
            int count = _messageStore.Count;
            if (pageSize < count)
                count = pageSize;
            lock (_syncLock)
            {
                results = _messageStore.Take(count);
                _messageStore.RemoveRange(0, count);
            }
            return results;
        }

        public void StoreMessages(ConcurrentQueue<IMessage> messages, string publisherId)
        {
            lock (_syncLock)
            {
                IMessage message;
                while (messages.TryDequeue(out message))
                {
                    _messageStore.AddRange(messages);
                }
            }
        }

        public void StoreMessages(IEnumerable<IMessage> messages, string publisherId)
        {
            lock (_syncLock)
            {
                _messageStore.AddRange(messages);
            }
        }
    }
}
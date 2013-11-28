using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace Burrows.BackedPublisher.BackingStores
{
    public class UnconfirmedMessageMemoryRepository : IUnconfirmedMessageRepository
    {
        private readonly List<Object> _messageStore = new List<Object>();
        private static readonly object _syncLock = new object();

        public IEnumerable<Object> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            IEnumerable<Object> results;
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

        public void StoreMessages(ConcurrentQueue<Object> messages, string publisherId)
        {
            lock (_syncLock)
            {
                Object message;
                while (messages.TryDequeue(out message))
                {
                    _messageStore.AddRange(messages);
                }
            }
        }

        public void StoreMessages(IEnumerable<Object> messages, string publisherId)
        {
            lock (_syncLock)
            {
                _messageStore.AddRange(messages);
            }
        }
    }
}
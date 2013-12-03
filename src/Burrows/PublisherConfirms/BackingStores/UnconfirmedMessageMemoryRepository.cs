using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Burrows.PublisherConfirms.BackingStores
{
    public class UnconfirmedMessageMemoryRepository : IUnconfirmedMessageRepository
    {
        private readonly List<ConfirmableMessage> _messageStore = new List<ConfirmableMessage>();
        private static readonly object _syncLock = new object();

        public Task<IList<ConfirmableMessage>> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            IList<ConfirmableMessage> results;
            int count = _messageStore.Count;
            if (pageSize < count)
                count = pageSize;
            lock (_syncLock)
            {
                results = _messageStore.Take(count).ToList();
                _messageStore.RemoveRange(0, count);
            }
            return Task.FromResult(results);
        }

        public Task StoreMessages(ConcurrentQueue<ConfirmableMessage> messages, string publisherId)
        {
            lock (_syncLock)
            {
                ConfirmableMessage message;
                while (messages.TryDequeue(out message))
                {
                    _messageStore.Add(message);
                }
            }
            return Task.FromResult(false);
        }

        public Task StoreMessages(IEnumerable<ConfirmableMessage> messages, string publisherId)
        {
            lock (_syncLock)
            {
                _messageStore.AddRange(messages);
            }
            return Task.FromResult(false);
        }
    }
}
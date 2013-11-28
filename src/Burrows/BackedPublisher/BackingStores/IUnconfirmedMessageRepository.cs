using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Burrows.BackedPublisher.BackingStores
{
    public interface IUnconfirmedMessageRepository
    {
        IEnumerable<Object> GetAndDeleteMessages(string publisherId, int pageSize);

        void StoreMessages(ConcurrentQueue<Object> messages, string publisherId);

        void StoreMessages(IEnumerable<Object> messages, string publisherId);
    }
}
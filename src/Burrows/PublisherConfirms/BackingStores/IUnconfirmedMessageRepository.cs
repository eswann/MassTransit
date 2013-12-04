using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Burrows.PublisherConfirms.BackingStores
{
    public interface IUnconfirmedMessageRepository
    {
        Task<IList<ConfirmableMessage>> GetAndDeleteMessages(string publisherId, int pageSize);

        Task StoreMessages(ConcurrentQueue<ConfirmableMessage> messages, string publisherId);
    }
}
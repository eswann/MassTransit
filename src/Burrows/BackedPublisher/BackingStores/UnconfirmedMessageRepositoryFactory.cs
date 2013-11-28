
namespace Burrows.BackedPublisher.BackingStores
{
    public class UnconfirmedMessageRepositoryFactory : IUnconfirmedMessageRepositoryFactory
    {
        private readonly PublishSettings _publishSettings;

        public UnconfirmedMessageRepositoryFactory(PublishSettings publishSettings)
        {
            _publishSettings = publishSettings;
        }

        public IUnconfirmedMessageRepository Create()
        {
            switch (_publishSettings.BackingStoreMethod)
            {
                case BackingStoreMethod.FileSystem:
                    return new UnconfirmedMessageFileRepository(_publishSettings);
                case BackingStoreMethod.SqlServer:
                    return new UnconfirmedMessageSqlRepository(_publishSettings);
                default:
                    return new UnconfirmedMessageMemoryRepository();
            }
        }
    }
}
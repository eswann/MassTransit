
namespace Burrows.PublisherConfirms.BackingStores
{
    public static class UnconfirmedMessageRepositoryFactory
    {
        public static IUnconfirmedMessageRepository Create(PublishSettings publishSettings)
        {
            switch (publishSettings.BackingStoreMethod)
            {
                case BackingStoreMethod.FileSystem:
                    return new UnconfirmedMessageFileRepository(publishSettings.FileRepositoryPath);
                default:
                    return new UnconfirmedMessageMemoryRepository();
            }
        }
    }
}
namespace Burrows.BackedPublisher.BackingStores
{
    public interface IUnconfirmedMessageRepositoryFactory
    {
        IUnconfirmedMessageRepository Create();
    }
}
namespace Burrows.PublisherConfirms.BackingStores
{
    public interface IUnconfirmedMessageRepositoryFactory
    {
        IUnconfirmedMessageRepository Create();
    }
}
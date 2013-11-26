namespace MassTransit.Transports.RabbitMq.Publish
{
    public class UnconfirmedMessageRepositoryFactory : IUnconfirmedMessageRepositoryFactory
    {
        private readonly IPublishSettings _publishSettings;

        public UnconfirmedMessageRepositoryFactory(IPublishSettings publishSettings)
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
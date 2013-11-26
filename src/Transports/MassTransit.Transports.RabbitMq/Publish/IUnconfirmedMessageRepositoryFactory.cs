namespace MassTransit.Transports.RabbitMq.Publish
{
    public interface IUnconfirmedMessageRepositoryFactory
    {
        IUnconfirmedMessageRepository Create();
    }
}
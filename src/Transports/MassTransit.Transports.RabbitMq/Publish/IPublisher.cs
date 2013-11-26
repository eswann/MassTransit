namespace MassTransit.Transports.RabbitMq.Publish
{
    public interface IPublisher
    {
        void Publish<T>(T message, bool force = false) where T : IMessage;
    }
}
namespace MassTransit.Transports.RabbitMq.Publish
{
    public enum BackingStoreMethod
    {
        InMemory = 0,
        SqlServer = 1,
        FileSystem = 2
    }
}
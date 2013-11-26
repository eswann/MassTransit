namespace MassTransit.Transports.RabbitMq.Publish
{
    public interface IPublishSettings
    {
        bool UsePublisherConfirms { get; set; }
        BackingStoreMethod BackingStoreMethod { get; set; }
        string PublisherId { get; set; }
        int MaxSuccessiveFailures { get; set; }
        int DeleteStoredMessagesBatchSize { get; set; }
        int InsertStoredMessagesBatchSize { get; set; }
        int GetStoredMessagesBatchSize { get; set; }
        int PublishRetryInterval { get; set; }
        int ProcessBufferedMessagesInterval { get; set; }
        int TimerCheckInterval { get; set; }
        string ConnectionStringName { get; set; }
        string ConnectionString { get; }
        string FileRepositoryPath { get; set; }
        void Validate();
        
    }
}
namespace MassTransit.Transports.RabbitMq.Publish
{
    using MassTransit.BusConfigurators;

    public static class ConfigurationExtensions
    {

        public static IPublishSettings UseRabbitWithPublisherConfirms(this ServiceBusConfigurator sbc, IComponentContext context, string publisherId)
        {
            var publishSettings = context.Resolve<IPublishSettings>();
            publishSettings.UsePublisherConfirms = true;
            publishSettings.PublisherId = publisherId;

            var confirmer = context.Resolve<IConfirmer>();

            sbc.UseRabbitMq(conf => conf.UsePublisherConfirms(confirmer.RecordRabbitPublication,
                                                              confirmer.ConfirmPublication,
                                                              confirmer.RecordPublicationFailure));
            return publishSettings;
        }

        public static IPublishSettings WithFileBackingStore(this IPublishSettings publishSettings,
                                                            string fileRepositoryPath = "MessageBackingStore")
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.FileSystem;
            publishSettings.FileRepositoryPath = fileRepositoryPath;

            return publishSettings;
        }

        public static IPublishSettings WithSqlBackingStore(this IPublishSettings publishSettings,
                                                           string connectionStringName)
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.SqlServer;
            publishSettings.ConnectionStringName = connectionStringName;

            return publishSettings;
        }

        public static IPublishSettings WithMaxSuccessiveFailures(this IPublishSettings publishSettings,
                                                                 int maxSuccessiveFailures)
        {
            publishSettings.MaxSuccessiveFailures = maxSuccessiveFailures;
            return publishSettings;
        }

        public static IPublishSettings WithDeleteBatchSize(this IPublishSettings publishSettings, int deleteBatchSize)
        {
            publishSettings.DeleteStoredMessagesBatchSize = deleteBatchSize;
            return publishSettings;
        }

        public static IPublishSettings WithInsertBatchSize(this IPublishSettings publishSettings, int insertBatchSize)
        {
            publishSettings.InsertStoredMessagesBatchSize = insertBatchSize;
            return publishSettings;
        }

        public static IPublishSettings WithGetBatchSize(this IPublishSettings publishSettings, int getBatchSize)
        {
            publishSettings.GetStoredMessagesBatchSize = getBatchSize;
            return publishSettings;
        }

        public static IPublishSettings WithPublishRetryInterval(this IPublishSettings publishSettings,
                                                                int intevalMilliseconds)
        {
            publishSettings.PublishRetryInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static IPublishSettings WithProcessBufferedMessagesInterval(this IPublishSettings publishSettings,
                                                        int intevalMilliseconds)
        {
            publishSettings.ProcessBufferedMessagesInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static IPublishSettings WithTimerCheckInteval(this IPublishSettings publishSettings,
                                                        int intevalMilliseconds)
        {
            publishSettings.TimerCheckInterval = intevalMilliseconds;
            return publishSettings;
        }

        
    }
}
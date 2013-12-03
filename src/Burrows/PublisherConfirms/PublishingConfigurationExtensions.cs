using Burrows.BusConfigurators;
using Burrows.PublisherConfirms.BackingStores;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.PublisherConfirms
{
    public static class PublishingConfigurationExtensions
    {

        public static PublishSettings UseRabbitWithPublisherConfirms(this ServiceBusConfigurator sbc, PublishSettings publishSettings)
        {
            publishSettings.UsePublisherConfirms = true;

            var confirmer = new Confirmer();

            sbc.UseRabbitMq(conf => conf.UsePublisherConfirms(confirmer.RecordPublicationSuccess, confirmer.RecordPublicationFailure));
            return publishSettings;
        }

        public static PublishSettings WithFileBackingStore(this PublishSettings publishSettings, string fileRepositoryPath = "MessageBackingStore")
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.FileSystem;
            publishSettings.FileRepositoryPath = fileRepositoryPath;

            return publishSettings;
        }

        public static PublishSettings WithSqlBackingStore(this PublishSettings publishSettings, string connectionString)
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.SqlServer;
            publishSettings.ConnectionString = connectionString;

            return publishSettings;
        }

        public static PublishSettings WithMaxSuccessiveFailures(this PublishSettings publishSettings,
                                                                 int maxSuccessiveFailures)
        {
            publishSettings.MaxSuccessiveFailures = maxSuccessiveFailures;
            return publishSettings;
        }

        public static PublishSettings WithDeleteBatchSize(this PublishSettings publishSettings, int deleteBatchSize)
        {
            publishSettings.DeleteStoredMessagesBatchSize = deleteBatchSize;
            return publishSettings;
        }

        public static PublishSettings WithInsertBatchSize(this PublishSettings publishSettings, int insertBatchSize)
        {
            publishSettings.InsertStoredMessagesBatchSize = insertBatchSize;
            return publishSettings;
        }

        public static PublishSettings WithGetBatchSize(this PublishSettings publishSettings, int getBatchSize)
        {
            publishSettings.GetStoredMessagesBatchSize = getBatchSize;
            return publishSettings;
        }

        public static PublishSettings WithPublishRetryInterval(this PublishSettings publishSettings,
                                                                int intevalMilliseconds)
        {
            publishSettings.PublishRetryInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static PublishSettings WithProcessBufferedMessagesInterval(this PublishSettings publishSettings,
                                                        int intevalMilliseconds)
        {
            publishSettings.ProcessBufferedMessagesInterval = intevalMilliseconds;
            return publishSettings;
        }

        public static PublishSettings WithTimerCheckInteval(this PublishSettings publishSettings,
                                                        int intevalMilliseconds)
        {
            publishSettings.TimerCheckInterval = intevalMilliseconds;
            return publishSettings;
        }

        
    }
}
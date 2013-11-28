using Burrows.BackedPublisher.BackingStores;
using Burrows.BusConfigurators;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.BackedPublisher
{
    public static class PublishingConfigurationExtensions
    {

        public static PublishSettings UseRabbitWithPublisherConfirms(this ServiceBusConfigurator sbc, PublishSettings publishSettings, IConfirmer confirmer)
        {
            publishSettings.UsePublisherConfirms = true;

            sbc.UseRabbitMq(conf => conf.UsePublisherConfirms());
            return publishSettings;
        }

        public static PublishSettings WithBackingStore(this PublishSettings publishSettings,
                                                            string fileRepositoryPath = "MessageBackingStore")
        {
            publishSettings.BackingStoreMethod = BackingStoreMethod.FileSystem;
            publishSettings.FileRepositoryPath = fileRepositoryPath;

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
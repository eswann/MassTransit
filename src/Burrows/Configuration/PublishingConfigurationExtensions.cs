using Burrows.Configuration.BusConfigurators;
using Burrows.PublisherConfirms;
using Burrows.PublisherConfirms.BackingStores;
using Burrows.Transports.Configuration.Extensions;

namespace Burrows.Configuration
{
    public static class PublishingConfigurationExtensions
    {

        public static PublishSettings UsePublisherConfirms(this IServiceBusConfigurator sbc, int testNacks = 0)
        {
            var confirmer = new Confirmer();
            var publishSettings = new PublishSettings {UsePublisherConfirms = true, Confirmer = confirmer};

            sbc.UseRabbitMq(conf => conf.UsePublisherConfirms(confirmer.RecordPublicationSuccess, confirmer.RecordPublicationFailure, testNacks));
            return publishSettings;
        }

        public static PublishSettings WithFileBackingStore(this PublishSettings publishSettings, string fileRepositoryPath = "MessageBackingStore")
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
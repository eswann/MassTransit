namespace MassTransit.Transports.RabbitMq.Publish
{
    using System;

    public class PublishSettings : IPublishSettings
    {

        private readonly ISettings _settings;

        private int _maxSuccessiveFailures = 5;
        private int _publishRetryInteval = 30000;
        private int _processBufferedMessagesInterval = 5000;
        private int _timerCheckInterval = 1000;
        private int _deleteStoredMessagesBatchSize = 500;
        private int _insertStoredMessagesBatchSize = 200;
        private int _getStoredMessagesBatchSize = 200;
        private string _connectionStringName;

        public PublishSettings(ISettings settings)
        {
            _settings = settings;
        }

        public bool UsePublisherConfirms { get; set; }

        public BackingStoreMethod BackingStoreMethod { get; set; }

        public string PublisherId { get; set; }

        public int MaxSuccessiveFailures
        {
            get { return _maxSuccessiveFailures; }
            set { _maxSuccessiveFailures = value; }
        }

        public int DeleteStoredMessagesBatchSize
        {
            get { return _deleteStoredMessagesBatchSize; }
            set { _deleteStoredMessagesBatchSize = value; }
        }

        public int InsertStoredMessagesBatchSize
        {
            get { return _insertStoredMessagesBatchSize; }
            set { _insertStoredMessagesBatchSize = value; }
        }

        public int GetStoredMessagesBatchSize
        {
            get { return _getStoredMessagesBatchSize; }
            set { _getStoredMessagesBatchSize = value; }
        }

        public int PublishRetryInterval
        {
            get { return _publishRetryInteval; }
            set { _publishRetryInteval = value; }
        }

        public int ProcessBufferedMessagesInterval
        {
            get { return _processBufferedMessagesInterval; }
            set { _processBufferedMessagesInterval = value; }
        }

        public int TimerCheckInterval
        {
            get { return _timerCheckInterval; }
            set { _timerCheckInterval = value; }
        }

        public string FileRepositoryPath { get; set; }

        public string ConnectionStringName
        {
            get { return _connectionStringName; }
            set
            {
                if (string.IsNullOrWhiteSpace(value))
                {
                    throw new ArgumentNullException("value", "Connection string name must be specified.");
                }

                _connectionStringName = value;
                if (_settings.ConnectionStrings(_connectionStringName) == null)
                {
                    throw new InvalidOperationException(string.Format("Connection string with the name {0} could not be found", _connectionStringName));
                }

                ConnectionString = _settings.ConnectionStrings(_connectionStringName).Value;
            }
        }

        public string ConnectionString { get; private set; }


        public void Validate()
        {
            if (!UsePublisherConfirms)
                return;

            if (BackingStoreMethod == BackingStoreMethod.SqlServer && string.IsNullOrWhiteSpace(ConnectionStringName))
            {
                throw new InvalidOperationException("ConnectionStringName must be specified.");
            }
            if (BackingStoreMethod == BackingStoreMethod.FileSystem && string.IsNullOrWhiteSpace(FileRepositoryPath))
            {
                throw new InvalidOperationException("FileRepositoryPath must be specified.");
            }
            if (string.IsNullOrWhiteSpace(PublisherId))
            {
                throw new InvalidOperationException("PublisherId must be specified.");
            }
            if (MaxSuccessiveFailures <= 0)
            {
                throw new InvalidOperationException("MaxSuccessiveFailures must be greater than 0.");
            }
            if (DeleteStoredMessagesBatchSize <= 0)
            {
                throw new InvalidOperationException("DeleteStoredMessagesBatchSize must be greater than 0.");
            }
            if (InsertStoredMessagesBatchSize <= 0)
            {
                throw new InvalidOperationException("InsertStoredMessagesBatchSize must be greater than 0.");
            }
            if (GetStoredMessagesBatchSize <= 0)
            {
                throw new InvalidOperationException("GetStoredMessagesBatchSize must be greater than 0.");
            }
            if (PublishRetryInterval <= 0)
            {
                throw new InvalidOperationException("PublishRetryInterval must be greater than 0.");
            }
            if (ProcessBufferedMessagesInterval <= 0)
            {
                throw new InvalidOperationException("PublishRetryInterval must be greater than 0.");
            }
            if (TimerCheckInterval <= 0)
            {
                throw new InvalidOperationException("PublishRetryInterval must be greater than 0.");
            }
        }
    }
}
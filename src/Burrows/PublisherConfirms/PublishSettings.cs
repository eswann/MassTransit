﻿using System;
using Burrows.PublisherConfirms.BackingStores;

namespace Burrows.PublisherConfirms
{
    public class PublishSettings
    {
        private int _maxSuccessiveFailures = 5;
        private int _publishRetryInteval = 30000;
        private int _processBufferedMessagesInterval = 5000;
        private int _timerCheckInterval = 1000;
        private int _getStoredMessagesBatchSize = 200;
        private string _fileRepositoryPath = "BurrowBackingStore";


        public bool UsePublisherConfirms { get; set; }

        public BackingStoreMethod BackingStoreMethod { get; set; }

        public string PublisherId { get; set; }

        public int MaxSuccessiveFailures
        {
            get { return _maxSuccessiveFailures; }
            set { _maxSuccessiveFailures = value; }
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

        public string FileRepositoryPath
        {
            get { return _fileRepositoryPath; }
            set { _fileRepositoryPath = value; }
        }


        public void Validate()
        {
            if (!UsePublisherConfirms)
                return;

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
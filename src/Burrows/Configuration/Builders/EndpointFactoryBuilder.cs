// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
//  
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use
// this file except in compliance with the License. You may obtain a copy of the 
// License at 
// 
//     http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software distributed
// under the License is distributed on an "AS IS" BASIS, WITHOUT WARRANTIES OR 
// CONDITIONS OF ANY KIND, either express or implied. See the License for the 
// specific language governing permissions and limitations under the License.

using Burrows.Endpoints;

namespace Burrows.Builders
{
    using System;
    using System.Collections.Generic;
    using System.Transactions;
    using EndpointConfigurators;
    using Serialization;
    using Transports;
    using Transports.Loopback;
    using Util;

    public interface IEndpointFactoryBuilder
    {
        /// <summary>
        /// Create the endpoint factory
        /// </summary>
        /// <returns></returns>
        IEndpointFactory Build();

        /// <summary>
        /// Sets the default serializer used for endpoints
        /// </summary>
        void SetDefaultSerializer(IMessageSerializer serializerFactory);

        /// <summary>
        /// Sets the default transaction timeout for transactional queue operations
        /// </summary>
        /// <param name="transactionTimeout"></param>
        void SetDefaultTransactionTimeout(TimeSpan transactionTimeout);

        /// <summary>
        /// Sets the flag indicating that missing queues should be created
        /// </summary>
        /// <param name="createMissingQueues"></param>
        void SetCreateMissingQueues(bool createMissingQueues);

        /// <summary>
        /// When creating queues, attempt to create transactional queues if available
        /// </summary>
        /// <param name="createTransactionalQueues"></param>
        void SetCreateTransactionalQueues(bool createTransactionalQueues);

        /// <summary>
        /// Specifies if the input queue should be purged on startup
        /// </summary>
        /// <param name="purgeOnStartup"></param>
        void SetPurgeOnStartup(bool purgeOnStartup);

        /// <summary>
        /// Provides a configured endpoint builder for the specified URI
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="endpointBuilder"></param>
        void AddEndpointBuilder(Uri uri, IEndpointBuilder endpointBuilder);

        /// <summary>
        /// Adds a transport factory to the builder
        /// </summary>
        void AddTransportFactory(ITransportFactory transportFactory);

        /// <summary>
        /// Add a message serializer to the builder so that messages with other serialization types
        /// can be received without conflicts
        /// </summary>
        /// <param name="serializer"></param>
        void AddMessageSerializer(IMessageSerializer serializer);

        /// <summary>
        /// Sets the default isolation level for transports that perform transactional operations
        /// </summary>
        void SetDefaultIsolationLevel(IsolationLevel isolationLevel);

        /// <summary>
        /// Sets the default retry limit for inbound messages
        /// </summary>
        void SetDefaultRetryLimit(int retryLimit);

        /// <summary>
        /// Sets the default message tracker factory for all endpoints
        /// </summary>
        void SetDefaultInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory);

        /// <summary>
        /// Sets the supported message serializers for all endpoints
        /// </summary>
        void SetSupportedMessageSerializers(ISupportedMessageSerializers supportedSerializers);
    }

    public class EndpointFactoryBuilder :
        IEndpointFactoryBuilder
    {
        private readonly EndpointFactoryDefaultSettings _defaults;
        private readonly IDictionary<Uri, IEndpointBuilder> _endpointBuilders;
        private readonly SupportedMessageSerializers _messageSerializers;
        private readonly IDictionary<string, ITransportFactory> _transportFactories;

        public EndpointFactoryBuilder([NotNull] IEndpointFactoryDefaultSettings defaults)
        {
            if (defaults == null)
                throw new ArgumentNullException("defaults");
            _endpointBuilders = new Dictionary<Uri, IEndpointBuilder>();
            _transportFactories = new Dictionary<string, ITransportFactory>();

            AddTransportFactory(new LoopbackTransportFactory());

            _defaults = new EndpointFactoryDefaultSettings(defaults);

            _messageSerializers = new SupportedMessageSerializers(_defaults.Serializer);

            _defaults.SupportedSerializers = _messageSerializers;
        }

        public IEndpointFactory Build()
        {
            var endpointFactory = new EndpointFactory(_transportFactories, _endpointBuilders, _defaults);

            return endpointFactory;
        }

        public void SetDefaultSerializer(IMessageSerializer defaultSerializer)
        {
            _defaults.Serializer = defaultSerializer;

            AddMessageSerializer(defaultSerializer);
        }

        public void SetDefaultTransactionTimeout(TimeSpan transactionTimeout)
        {
            _defaults.TransactionTimeout = transactionTimeout;
        }

        public void SetDefaultIsolationLevel(IsolationLevel isolationLevel)
        {
            _defaults.IsolationLevel = isolationLevel;
        }

        public void SetDefaultRetryLimit(int retryLimit)
        {
            _defaults.RetryLimit = retryLimit;
        }

        public void SetDefaultInboundMessageTrackerFactory(MessageTrackerFactory messageTrackerFactory)
        {
            _defaults.TrackerFactory = messageTrackerFactory;
        }

        public void SetCreateMissingQueues(bool createMissingQueues)
        {
            _defaults.CreateMissingQueues = createMissingQueues;
        }

        public void SetCreateTransactionalQueues(bool createTransactionalQueues)
        {
            _defaults.CreateTransactionalQueues = createTransactionalQueues;
        }

        public void SetPurgeOnStartup(bool purgeOnStartup)
        {
            _defaults.PurgeOnStartup = purgeOnStartup;
        }

        public void SetSupportedMessageSerializers(ISupportedMessageSerializers supportedSerializers)
        {
            _defaults.SupportedSerializers = supportedSerializers;
        }

        public void AddEndpointBuilder(Uri uri, IEndpointBuilder endpointBuilder)
        {
            _endpointBuilders[uri] = endpointBuilder;
        }

        public void AddTransportFactory(ITransportFactory transportFactory)
        {
            string scheme = transportFactory.Scheme.ToLowerInvariant();

            _transportFactories[scheme] = transportFactory;
        }

        public void AddMessageSerializer(IMessageSerializer serializer)
        {
            _messageSerializers.AddSerializer(serializer);
        }
    }
}
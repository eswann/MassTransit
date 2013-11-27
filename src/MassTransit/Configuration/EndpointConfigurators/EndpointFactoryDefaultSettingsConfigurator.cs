﻿// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Burrows.EndpointConfigurators
{
    using System;
    using Serialization;

    public interface IEndpointFactoryDefaultSettingsConfigurator
    {
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
    }

	public class EndpointFactoryDefaultSettingsConfigurator :
		IEndpointFactoryDefaultSettingsConfigurator
	{
		readonly EndpointFactoryDefaultSettings _defaults;

		public EndpointFactoryDefaultSettingsConfigurator(EndpointFactoryDefaultSettings defaults)
		{
			_defaults = defaults;
		}

		public void SetDefaultSerializer(IMessageSerializer defaultSerializer)
		{
			_defaults.Serializer = defaultSerializer;
		}

		public void SetDefaultTransactionTimeout(TimeSpan transactionTimeout)
		{
			_defaults.TransactionTimeout = transactionTimeout;
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
	}
}
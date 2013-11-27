﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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

namespace Burrows.EndpointConfigurators
{
    using System;
    using System.Transactions;
    using Serialization;
    using Transports;
    using Util;

    public interface IEndpointFactoryDefaultSettings
    {
        MessageTrackerFactory TrackerFactory { get; }
        IMessageSerializer Serializer { get; }
        ISupportedMessageSerializers SupportedSerializers { get; }
        bool CreateMissingQueues { get; }
        bool CreateTransactionalQueues { get; }
        bool PurgeOnStartup { get; }
        TimeSpan TransactionTimeout { get; }
        bool RequireTransactional { get; }
        IsolationLevel IsolationLevel { get; }
        int RetryLimit { get; }

        [NotNull]
        EndpointSettings CreateEndpointSettings([NotNull] IEndpointAddress address);
    }
}
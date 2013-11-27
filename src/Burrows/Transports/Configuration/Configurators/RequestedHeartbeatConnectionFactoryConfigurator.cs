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
namespace Burrows.Transports.Configuration.Configurators
{
    using System.Collections.Generic;
    using Builders;
    using Burrows.Configurators;
    using RabbitMQ.Client;

    public class RequestedHeartbeatConnectionFactoryConfigurator : IConnectionFactoryBuilderConfigurator
    {
        private readonly ushort _requestedHeartbeat;

        public RequestedHeartbeatConnectionFactoryConfigurator(ushort requestedHeartbeat)
        {
            _requestedHeartbeat = requestedHeartbeat;
        }

        public IConnectionFactoryBuilder Configure(IConnectionFactoryBuilder builder)
        {
            builder.Add(Configure);
            return builder;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            yield break;
        }

        ConnectionFactory Configure(ConnectionFactory connectionFactory)
        {
            connectionFactory.RequestedHeartbeat = _requestedHeartbeat;
            return connectionFactory;
        }
    }
}
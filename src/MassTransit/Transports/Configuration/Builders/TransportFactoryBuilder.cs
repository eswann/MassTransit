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
namespace MassTransit.Transports.Configuration.Builders
{
    using System;
    using System.Collections.Generic;
    using Publish;

    public interface ITransportFactoryBuilder
    {
        void AddConnectionFactoryBuilder(Uri uri, IConnectionFactoryBuilder connectionFactoryBuilder);

        void SetPublisherConfirmSettings(IPublisherConfirmSettings publisherConfirmSettings);
    }

    public class TransportFactoryBuilder :
        ITransportFactoryBuilder
    {
        private readonly IDictionary<Uri, IConnectionFactoryBuilder> _connectionFactoryBuilders;
        IPublisherConfirmSettings _publisherConfirmSettings = new PublisherConfirmSettings();

        public TransportFactoryBuilder()
        {
            _connectionFactoryBuilders = new Dictionary<Uri, IConnectionFactoryBuilder>();
        }

        public void AddConnectionFactoryBuilder(Uri uri, IConnectionFactoryBuilder connectionFactoryBuilder)
        {
            _connectionFactoryBuilders[uri] = connectionFactoryBuilder;
        }


        public void SetPublisherConfirmSettings(IPublisherConfirmSettings publisherConfirmSettings)
        {
            _publisherConfirmSettings = publisherConfirmSettings;
        }

        public TransportFactory Build()
        {
            var factory = new TransportFactory(_connectionFactoryBuilders, _publisherConfirmSettings);

            return factory;
        }
    }
}
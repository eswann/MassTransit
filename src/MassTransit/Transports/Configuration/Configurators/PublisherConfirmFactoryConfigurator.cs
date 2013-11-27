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
namespace Burrows.Transports.Configuration.Configurators
{
    using System.Collections.Generic;
    using Builders;
    using Burrows.Configurators;
    using Publish;

    /// <summary>
    /// Configures SSL/TLS for RabbitMQ. See http://www.rabbitmq.com/ssl.html
    /// for details on how to set up RabbitMQ for SSL.
    /// </summary>
    public interface IPublisherConfirmFactoryConfigurator
    {
    }

    public class PublisherConfirmFactoryConfigurator :
        IPublisherConfirmFactoryConfigurator,
        ITransportFactoryBuilderConfigurator
    {
        private readonly bool _usePublisherConfirms;

        public PublisherConfirmFactoryConfigurator(bool usePublisherConfirms)
        {
            _usePublisherConfirms = usePublisherConfirms;
        }

        public ITransportFactoryBuilder Configure(ITransportFactoryBuilder builder)
        {
            builder.SetPublisherConfirmSettings(new PublisherConfirmSettings{ UsePublisherConfirms = _usePublisherConfirms });

            return builder;
        }

        public IEnumerable<IValidationResult> Validate()
        {
            return null;
        }
    }
}
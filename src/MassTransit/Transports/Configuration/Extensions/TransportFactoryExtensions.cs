// Copyright 2007-2011 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Burrows.Transports.Configuration.Extensions
{
    using System;
    using Configurators;

    public static class TransportFactoryExtensions
	{
		public static void ConfigureHost(this ITransportFactoryConfigurator configurator, Uri hostAddress,
		                                 Action<IConnectionFactoryConfigurator> configureHost)
		{
			var hostConfigurator = new ConnectionFactoryConfigurator(RabbitMqEndpointAddress.Parse(hostAddress));
			configureHost(hostConfigurator);

			configurator.AddConfigurator(hostConfigurator);
		}

        public static void UsePublisherConfirms(this ITransportFactoryConfigurator configurator)
        {
            var hostConfigurator = new PublisherConfirmFactoryConfigurator(true);

            configurator.AddConfigurator(hostConfigurator);
        }
	}
}
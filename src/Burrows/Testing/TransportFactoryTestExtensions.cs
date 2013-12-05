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

using Burrows.Configuration;

namespace Burrows.Testing
{
    using Configurators;
    using ScenarioConfigurators;
    using Transports;

    public static class TransportFactoryTestExtensions
	{
		public static void AddTransportFactory<TTransportFactory>(this IScenarioConfigurator<IBusTestScenario> configurator)
			where TTransportFactory : class, ITransportFactory, new()
		{
			var endpointFactoryConfigurator =
				new EndpointTestScenarioBuilderConfigurator<IBusTestScenario>(x => x.AddTransportFactory<TTransportFactory>());

			configurator.AddConfigurator(endpointFactoryConfigurator);
		}

		public static void AddTransportFactory<TTransportFactory>(
			this IScenarioConfigurator<ILocalRemoteTestScenario> configurator)
			where TTransportFactory : class, ITransportFactory, new()
		{
			var endpointFactoryConfigurator =
				new EndpointTestScenarioBuilderConfigurator<ILocalRemoteTestScenario>(
					x => x.AddTransportFactory<TTransportFactory>());

			configurator.AddConfigurator(endpointFactoryConfigurator);
		}
	}
}
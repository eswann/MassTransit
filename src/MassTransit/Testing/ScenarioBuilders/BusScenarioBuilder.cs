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
namespace MassTransit.Testing.ScenarioBuilders
{
	using System;
	using Advanced;
	using BusConfigurators;
	using Diagnostics;
	using Magnum.Extensions;
	using Scenarios;
	using SubscriptionConfigurators;
	using Transports;
	using Transports.Configuration.Extensions;

    public interface IBusScenarioBuilder :
    IEndpointScenarioBuilder<IBusTestScenario>
    {
        /// <summary>
        /// Configure any bus-specific items as part of building the test scenario
        /// </summary>
        /// <param name="configureCallback"></param>
        void ConfigureBus(Action<IServiceBusConfigurator> configureCallback);

        /// <summary>
        /// Configure the subscriptions for a test using this scenario.
        /// </summary>
        /// <param name="configureCallback"></param>
        void ConfigureSubscriptions(Action<ISubscriptionBusServiceConfigurator> configureCallback);
    }

	/// <summary>
	/// Implementation for the test scenario, but abstract for others to customize it. Sets some defaults in the c'tor, which you
	/// can override with the <see cref="ConfigureBus"/> and <see cref="ConfigureSubscriptions"/> methods.
	/// </summary>
	public class BusScenarioBuilder :
		EndpointScenarioBuilder<IBusTestScenario>,
		IBusScenarioBuilder
	{
		readonly ServiceBusConfigurator _configurator;
		readonly ServiceBusDefaultSettings _settings;

        const string DefaultUri = "rabbitmq://localhost/mt_client";

        public BusScenarioBuilder() : this(new Uri(DefaultUri))
        {
		}

		/// <summary>
		/// c'tor
		/// </summary>
		/// <param name="uri">The uri to receive from during the scenario.</param>
		protected BusScenarioBuilder(Uri uri)
		{
            _settings = new ServiceBusDefaultSettings { ConcurrentConsumerLimit = 1, ReceiveTimeout = 50.Milliseconds() };

            _configurator = new ServiceBusConfigurator(_settings);
            _configurator.ReceiveFrom(uri);

            ConfigureEndpointFactory(x => x.UseRabbitMq());

            ConfigureBus(x =>
            {
                x.UseRabbitMq();
                x.SetReceiveTimeout(100.Milliseconds());
            });
		}

		public void ConfigureBus(Action<IServiceBusConfigurator> configureCallback)
		{
			configureCallback(_configurator);
		}

		public void ConfigureSubscriptions(Action<ISubscriptionBusServiceConfigurator> configureCallback)
		{
			_configurator.Subscribe(configureCallback);
		}

		public override IBusTestScenario Build()
		{
			IEndpointFactory endpointFactory = BuildEndpointFactory();

			var scenario = new BusTestScenario(endpointFactory);

			_configurator.ChangeSettings(x => { x.EndpointCache = scenario.EndpointCache; });
			_configurator.EnableMessageTracing();

			scenario.Bus = _configurator.CreateServiceBus();

			return scenario;
		}
	}
}
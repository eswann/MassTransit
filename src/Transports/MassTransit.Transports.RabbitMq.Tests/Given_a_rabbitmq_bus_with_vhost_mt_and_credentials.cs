﻿namespace MassTransit.Transports.RabbitMq.Tests
{
	using System;
	using Configuration;
	using Configuration.Extensions;
	using Magnum.TestFramework;
	using TestFramework.Fixtures;

	[Scenario]
	public class Given_a_rabbitmq_bus_with_vhost_mt_and_credentials :
		LocalTestFixture<TransportFactory>
	{
		protected Given_a_rabbitmq_bus_with_vhost_mt_and_credentials()
		{
			LocalUri = new Uri("rabbitmq://testUser:topSecret@localhost:5672/mt/test_queue");
			ConfigureEndpointFactory(x => x.UseJsonSerializer());
		}

		protected override void ConfigureServiceBus(Uri uri, BusConfigurators.IServiceBusConfigurator configurator)
		{
			base.ConfigureServiceBus(uri, configurator);
			configurator.UseRabbitMq();
		}
	}
}
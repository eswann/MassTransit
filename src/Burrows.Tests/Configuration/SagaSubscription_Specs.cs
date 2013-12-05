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
using Burrows.Tests.Framework;

namespace Burrows.Tests.Configuration
{
    using Burrows.Saga.Configuration;
    using Examples.Sagas;
    using Examples.Sagas.Messages;
    using Magnum.TestFramework;
    using Burrows.Saga;
    using Burrows.Services.Subscriptions.Messages;
    using Burrows.Services.Subscriptions.Server;
    using Burrows.Services.Subscriptions.Server.Messages;
    using Rhino.Mocks;

    [Scenario]
	public class When_subscribing_a_saga_to_the_bus
	{
		IServiceBus _bus;

		[When]
		public void Subscribing_a_consumer_to_the_bus()
		{
			_bus = ServiceBusFactory.New(x =>
				{
					x.ReceiveFrom("loopback://localhost/mt_test");

					x.Subscribe(s => s.Saga(MockRepository.GenerateMock<ISagaRepository<SimpleStateMachineSaga>>()));
				});
		}

		[Finally]
		public void Finally()
		{
			_bus.Dispose();
		}

		[Then]
		public void Should_have_subscribed_start_message()
		{
			_bus.ShouldHaveSubscriptionFor<StartSimpleSaga>();
		}

		[Then]
		public void Should_have_subscribed_approve_message()
		{
			_bus.ShouldHaveSubscriptionFor<ApproveSimpleCustomer>();
		}

		[Then]
		public void Should_have_subscribed_finish_message()
		{
			_bus.ShouldHaveSubscriptionFor<FinishSimpleSaga>();
		}
	}

	[Scenario]
	public class When_subscribing_a_complex_saga_to_the_bus
	{
		IServiceBus _bus;

		[When]
		public void Subscribing_a_complex_saga_to_the_bus()
		{
			_bus = ServiceBusFactory.New(x =>
				{
					x.ReceiveFrom("loopback://localhost/mt_test");

					x.Subscribe(s => s.Saga(MockRepository.GenerateMock<ISagaRepository<SubscriptionSaga>>()));
				});
		}

		[Finally]
		public void Finally()
		{
			_bus.Dispose();
		}

		[Then]
		public void Should_have_add_subscription()
		{
			_bus.ShouldHaveSagaSubscriptionFor<SubscriptionSaga, AddSubscription>(typeof(CreateOrUseExistingSagaPolicy<,>));
		}

		[Then]
		public void Should_have_remove_subscription()
		{
			_bus.ShouldHaveSagaSubscriptionFor<SubscriptionSaga, RemoveSubscription>(typeof(ExistingOrIgnoreSagaPolicy<,>));
		}

		[Then]
		public void Should_have_subscription_client_added()
		{
			_bus.ShouldHaveSagaSubscriptionFor<SubscriptionSaga, SubscriptionClientAdded>(typeof(ExistingOrIgnoreSagaPolicy<,>));
		}

		[Then]
		public void Should_have_duplicate_subscription_client_removed()
		{
			_bus.ShouldHaveSagaSubscriptionFor<SubscriptionSaga, DuplicateSubscriptionClientRemoved>(typeof(ExistingOrIgnoreSagaPolicy<,>));
		}
	}
}
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
namespace Burrows.SubscriptionBuilders
{
    using System.Collections.Generic;
    using Subscriptions;

    public interface ISubscriptionBusServiceBuilder
    {
        /// <summary>
        /// Add a subscription builder to the service so that it is subscribed when
        /// the bus is started.
        /// </summary>
        /// <param name="builder"></param>
        void AddSubscriptionBuilder(ISubscriptionBuilder builder);
    }

	public class SubscriptionBusServiceBuilder :
		ISubscriptionBusServiceBuilder
	{
		readonly IList<ISubscriptionBuilder> _builders;

		public SubscriptionBusServiceBuilder()
		{
			_builders = new List<ISubscriptionBuilder>();
		}

		public void AddSubscriptionBuilder(ISubscriptionBuilder builder)
		{
			_builders.Add(builder);
		}

		public IBusService Build()
		{
			return new SubscriptionBusService(_builders);
		}
	}
}
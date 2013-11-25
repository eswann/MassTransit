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
namespace MassTransit.Testing.ScenarioConfigurators
{
	using System;
	using System.Collections.Generic;
	using Configurators;
	using EndpointConfigurators;
	using ScenarioBuilders;
	using Scenarios;

	public class EndpointTestScenarioBuilderConfigurator<TScenario> :
		IScenarioBuilderConfigurator<TScenario>
		where TScenario : IEndpointTestScenario
	{
		readonly Action<IEndpointFactoryConfigurator> _configureAction;

		public EndpointTestScenarioBuilderConfigurator(Action<IEndpointFactoryConfigurator> configureAction)
		{
			_configureAction = configureAction;
		}

		public IEnumerable<ITestConfiguratorResult> Validate()
		{
			yield break;
		}

		public IScenarioBuilder<TScenario> Configure(IScenarioBuilder<TScenario> builder)
		{
			var endpointBuilder = builder as IEndpointScenarioBuilder<TScenario>;
			if (endpointBuilder != null)
			{
				endpointBuilder.ConfigureEndpointFactory(_configureAction);
			}

			return builder;
		}
	}
}
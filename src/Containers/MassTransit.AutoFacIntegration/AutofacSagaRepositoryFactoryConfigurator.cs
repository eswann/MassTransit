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
namespace Burrows.AutofacIntegration
{
    using System;
    using Autofac;
    using Magnum.Reflection;
    using Saga;
    using Saga.Configuration;
    using SubscriptionConfigurators;
    using Util;

    public class AutofacSagaRepositoryFactoryConfigurator
	{
		private readonly ISubscriptionBusServiceConfigurator _configurator;
        private readonly ILifetimeScope _scope;
        private readonly string _name;

	    public AutofacSagaRepositoryFactoryConfigurator(ISubscriptionBusServiceConfigurator configurator, ILifetimeScope scope, string name)
		{
            _scope = scope;
	        _name = name;
	        _configurator = configurator;
		}

		public void ConfigureSaga(Type sagaType)
		{
			this.FastInvoke(new[] {sagaType}, "Configure");
		}

		[UsedImplicitly]
		public void Configure<T>()
			where T : class, ISaga
		{
		    var sagaRepository = _scope.Resolve<ISagaRepository<T>>();

		    var autofacSagaRepository = new AutofacSagaRepository<T>(sagaRepository, _scope, _name);
		    _configurator.Saga(autofacSagaRepository);
		}
	}
}
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
namespace Burrows.Pipeline
{
    using System;
    using System.Collections.Generic;
    using Context;
    using Magnum.StateMachine;
    using Saga;

    public delegate IEnumerable<Action<IConsumeContext<TMessage>>> InstanceHandlerSelector<TInstance, TMessage>(
		TInstance instance, IConsumeContext<TMessage> context)
		where TMessage : class;


	public static class InstanceHandlerSelector
	{
		public static IEnumerable<Action<IConsumeContext<TMessage>>> ForInitiatedBy<TInstance, TMessage>(TInstance instance)
			where TInstance : InitiatedBy<TMessage>
			where TMessage : class, ICorrelatedBy<Guid>
		{
			yield return x =>
				{
					using (x.CreateScope())
					{
						instance.Consume(x.Message);
					}
				};
		}

		public static IEnumerable<Action<IConsumeContext<TMessage>>> ForDataEvent<TInstance, TMessage>(TInstance instance,
		                                                                                               Event<TMessage> eevent)
			where TInstance : SagaStateMachine<TInstance>
			where TMessage : class, ICorrelatedBy<Guid>
		{
			yield return x =>
				{
					using (x.CreateScope())
					{
						instance.RaiseEvent(eevent, x.Message);
					}
				};
		}
	}
}
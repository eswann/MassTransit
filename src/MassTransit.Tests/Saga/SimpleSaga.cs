// Copyright 2007-2008 The Apache Software Foundation.
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
namespace MassTransit.Tests.Saga
{
	using System;
	using System.Linq.Expressions;
	using MassTransit.Saga;

	public class SimpleSaga :
		InitiatedBy<InitiateSimpleSaga>,
		IOrchestrate<CompleteSimpleSaga>,
		IObserve<ObservableSagaMessage,SimpleSaga>,
		ISaga
	{
		public SimpleSaga()
		{
		}

		public SimpleSaga(Guid correlationId)
		{
			CorrelationId = correlationId;
		}

		public bool Completed { get; private set; }
		public bool Initiated { get; private set; }
		public bool Observed { get; private set; }
		public string Name { get; private set; }

		public Guid Id
		{
			get { return CorrelationId; }
		}

		public void Consume(InitiateSimpleSaga message)
		{
			Initiated = true;
			Name = message.Name;
		}

		public Guid CorrelationId { get; set; }

		public IServiceBus Bus { get; set; }

		public void Consume(ObservableSagaMessage message)
		{
			Observed = true;
		}

		public Expression<Func<ObservableSagaMessage, bool>> BindExpression
		{
			get { return message => message.Name == Name; }
		}

		public void Consume(CompleteSimpleSaga message)
		{
			Completed = true;
		}

		public Expression<Func<SimpleSaga, ObservableSagaMessage, bool>> GetBindExpression()
		{
			return (saga, message) => saga.Name == message.Name;
		}
	}
}
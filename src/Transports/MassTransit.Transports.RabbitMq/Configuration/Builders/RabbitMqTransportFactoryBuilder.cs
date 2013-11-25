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
namespace MassTransit.Transports.RabbitMq.Configuration.Builders
{
	using System;
	using System.Collections.Generic;

    public interface IRabbitMqTransportFactoryBuilder
    {
        void AddConnectionFactoryBuilder(Uri uri, IConnectionFactoryBuilder connectionFactoryBuilder);
    }

	public class RabbitMqTransportFactoryBuilder :
		IRabbitMqTransportFactoryBuilder
	{
		readonly IDictionary<Uri, IConnectionFactoryBuilder> _connectionFactoryBuilders;

		public RabbitMqTransportFactoryBuilder()
		{
			_connectionFactoryBuilders = new Dictionary<Uri, IConnectionFactoryBuilder>();
		}

		public void AddConnectionFactoryBuilder(Uri uri, IConnectionFactoryBuilder connectionFactoryBuilder)
		{
			_connectionFactoryBuilders[uri] = connectionFactoryBuilder;
		}

		public RabbitMqTransportFactory Build()
		{
			var factory = new RabbitMqTransportFactory(_connectionFactoryBuilders);

			return factory;
		}
	}
}
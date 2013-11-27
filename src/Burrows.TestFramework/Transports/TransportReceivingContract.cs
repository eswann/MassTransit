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

using Burrows.Endpoints;

namespace Burrows.TestFramework.Transports
{
    using System;
    using Burrows.Transports;
    using Messages;
    using NUnit.Framework;

    public abstract class TransportReceivingContract<TTransportFactory>
		where TTransportFactory : class, ITransportFactory, new()
	{
		IEndpoint _endpoint;

		protected TransportReceivingContract(Uri uri)
		{
			Address = uri;
		}

		public Uri Address { get; set; }
		public Action<Uri> VerifyMessageIsNotInQueue { get; set; }


		[SetUp]
		public void SetUp()
		{
			IEndpointCache endpointCache = EndpointCacheFactory.New(x => x.AddTransportFactory<TTransportFactory>());

			_endpoint = endpointCache.GetEndpoint(Address);
		}

		[TearDown]
		public void TearDown()
		{
			_endpoint.Dispose();
			_endpoint = null;
		}


		public void VerifyMessageIsInQueue(IEndpoint ep)
		{
			ep.ShouldContain<DeleteMessage>();
		}
	}
}
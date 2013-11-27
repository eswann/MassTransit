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

namespace Burrows.Context
{
	public class Sent :
		ISent
	{
		readonly IEndpointAddress _address;
		readonly ISendContext _context;
	    readonly long _timestamp;

		public Sent(ISendContext context, IEndpointAddress address, long timestamp)
		{
			_timestamp = timestamp;
			_context = context;
			_address = address;
		}

		public ISendContext Context
		{
			get { return _context; }
		}

		public long Timestamp
		{
			get { return _timestamp; }
		}

		public IEndpointAddress Address
		{
			get { return _address; }
		}
	}

	public class Sent<T> :
		ISent
		where T : class
	{
		readonly IEndpointAddress _address;
		readonly ISendContext<T> _context;
	    readonly long _timestamp;

		public Sent(ISendContext<T> context, IEndpointAddress address, long timestamp)
		{
			_timestamp = timestamp;
			_context = context;
			_address = address;
		}

		public ISendContext Context
		{
			get { return _context; }
		}

		public long Timestamp
		{
			get { return _timestamp; }
		}

		public IEndpointAddress Address
		{
			get { return _address; }
		}
	}
}
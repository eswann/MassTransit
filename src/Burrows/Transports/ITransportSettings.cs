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

namespace Burrows.Transports
{
    using System;
    using System.Transactions;

    public interface ITransportSettings
	{
		/// <summary>
		/// The address of the endpoint/transport
		/// </summary>
		IEndpointAddress Address { get; }

		/// <summary>
		/// The transport should be created if it was not found
		/// </summary>
		bool CreateIfMissing { get; }

		/// <summary>
		/// If the transport should purge any existing messages before reading from the queue
		/// </summary>
		bool PurgeExistingMessages { get; }
	}
}
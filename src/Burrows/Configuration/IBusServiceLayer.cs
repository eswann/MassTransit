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
namespace Burrows.Configuration
{
	/// <summary>
	/// The layer of the service, which controls the order in which it is started and stopped
	/// </summary>
	public enum IBusServiceLayer
	{
		/// <summary>
		/// The lowest level, and includes things that sit just above transports
		/// </summary>
		Network = 0,

		/// <summary>
		/// The next level, which includes subscription, timeout, health, and other services that must
		/// be running before the application can start.
		/// </summary>
		Session,

		/// <summary>
		/// The next level, which includes services that sit on top of the session services but not yet
		/// application level services, such as the distributor and workers
		/// </summary>
		Presentation,

		/// <summary>
		/// The highest level, which includes an application level services, such as subscriptions
		/// </summary>
		Application
	}
}
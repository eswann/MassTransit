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
namespace Burrows.Services.HealthMonitoring.Messages
{
    using System;

    [Serializable]
	public class HealthInformation
	{
		public HealthInformation(Guid clientId, Uri controlUri, Uri dataUri, DateTime lastHeartbeat, string state)
		{
			ClientId = clientId;
			ControlUri = controlUri;
			DataUri = dataUri;
			LastHeartbeat = lastHeartbeat;
			State = state;
		}

		protected HealthInformation()
		{
		}

		public Guid ClientId { get; set; }
		public Uri ControlUri { get; set; }
		public Uri DataUri { get; set; }
		public DateTime LastHeartbeat { get; set; }
		public string State { get; set; }
	}
}
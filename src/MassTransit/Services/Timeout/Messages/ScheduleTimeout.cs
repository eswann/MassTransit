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
namespace MassTransit.Services.Timeout.Messages
{
    using System;
    using System.Data.SqlTypes;

    [Serializable]
	public class ScheduleTimeout : 
		TimeoutMessageBase
	{
		protected ScheduleTimeout()
		{
		}

		public ScheduleTimeout(Guid correlationId, TimeSpan timeoutIn)
			: this(correlationId, DateTime.UtcNow + timeoutIn)
		{
		}

		public ScheduleTimeout(Guid correlationId, TimeSpan timeoutIn, int tag)
			: this(correlationId, DateTime.UtcNow + timeoutIn, tag)
		{
		}

		public ScheduleTimeout(Guid correlationId, DateTime timeoutAt)
			: this(correlationId, timeoutAt, 0)
		{
		}

		public ScheduleTimeout(Guid correlationId, DateTime timeoutAt, int tag)
		{
			if(timeoutAt < SqlDateTime.MinValue || timeoutAt > SqlDateTime.MaxValue)
				throw new ArgumentException("The scheduled time must be between " + SqlDateTime.MinValue + " and " + SqlDateTime.MaxValue);

			CorrelationId = correlationId;
			TimeoutAt = timeoutAt.ToUniversalTime();
			Tag = tag;
		}
	}
}
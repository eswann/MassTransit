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

using System;
using Burrows.Tests.Framework.Fixtures;

namespace Burrows.Tests.Framework
{
    public class SelectiveConsumerOf<TMessage> :
		AbstractTestConsumer<TMessage>,
		Consumes<TMessage>.Selected
		where TMessage : class
	{
		private readonly Predicate<TMessage> _accept;

		public SelectiveConsumerOf(Predicate<TMessage> accept)
		{
			_accept = accept;
		}

		public SelectiveConsumerOf()
		{
			_accept = x => true;
		}

		public bool Accept(TMessage message)
		{
			return _accept(message);
		}
	}
}
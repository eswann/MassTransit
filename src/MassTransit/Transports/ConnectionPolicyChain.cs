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
namespace MassTransit.Transports
{
	using System;
	using System.Collections.Generic;

    public interface IConnectionPolicyChain
    {
        void Push(IConnectionPolicy policy);

        void Pop(IConnectionPolicy policy);

        void Next(Action callback);
    }

	public class ConnectionPolicyChain :
		IConnectionPolicyChain
	{
		readonly Stack<IConnectionPolicy> _policies;

		public ConnectionPolicyChain(IConnectionHandler connectionHandler)
		{
			_policies = new Stack<IConnectionPolicy>();
			_policies.Push(new DefaultConnectionPolicy(connectionHandler));
		}

		public void Push(IConnectionPolicy policy)
		{
			lock (_policies)
				_policies.Push(policy);
		}

		public void Pop(IConnectionPolicy policy)
		{
			lock (_policies)
				if (_policies.Peek() == policy)
					_policies.Pop();
		}

		public void Next(Action callback)
		{
			IConnectionPolicy policy;

			lock (_policies)
				policy = _policies.Peek();

			policy.Execute(callback);
		}

		public void Execute(Action callback)
		{
			Next(callback);
		}

		public void Set(IConnectionPolicy connectionPolicy)
		{
			lock (_policies)
			{
				_policies.Clear();
				_policies.Push(connectionPolicy);
			}
		}
	}
}
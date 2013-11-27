// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
namespace Burrows.RequestResponse
{
    using System;
    using System.Threading;

    public class FaultHandler<TRequest>
        where TRequest : class
    {
        private readonly Action<TRequest, Fault<TRequest>> _faultCallback;
        private readonly SynchronizationContext _synchronizationContext;

        public FaultHandler(SynchronizationContext synchronizationSynchronizationContext,
            Action<TRequest, Fault<TRequest>> faultCallback)
        {
            _faultCallback = faultCallback;
            _synchronizationContext = synchronizationSynchronizationContext;
        }

        public void HandleFault(TRequest request, Fault<TRequest> fault)
        {
            if (_synchronizationContext != null)
            {
                _synchronizationContext.Post(state => _faultCallback(request, fault), state: null);
            }
            else
            {
                _faultCallback(request, fault);
            }
        }
    }
}
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
namespace Burrows.Containers.Tests.Scenarios
{
    using System;
    using System.Threading;
    using Magnum.Extensions;

    public interface IAnotherMessageConsumer :
        Consumes<IAnotherMessageInterface>.All
    {
        IAnotherMessageInterface Last { get; }
    }

    public class AnotherMessageConsumer :
       IAnotherMessageConsumer
    {
        readonly ManualResetEvent _received;
        IAnotherMessageInterface _last;

        public AnotherMessageConsumer()
        {
            Console.WriteLine("AnotherMessageConsumer()");

            _received = new ManualResetEvent(false);
        }

        public IAnotherMessageInterface Last
        {
            get
            {
                if (_received.WaitOne(8.Seconds()))
                    return _last;

                throw new TimeoutException("Timeout waiting for message to be consumed");
            }
        }

        public void Consume(IAnotherMessageInterface message)
        {
            _last = message;
            _received.Set();
        }
    }
}
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
namespace MassTransit.Testing
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Magnum.Extensions;

    public interface ISentMessageList :
        IEnumerable<ISentMessage>
    {
        bool Any();
        bool Any(Func<ISentMessage, bool> filter);

        bool Any<T>()
            where T : class;

        bool Any<T>(Func<ISentMessage<T>, bool> filter)
            where T : class;
    }

    public class SentMessageList :
        ISentMessageList,
        IDisposable
    {
        private readonly HashSet<ISentMessage> _messages;
        private readonly AutoResetEvent _received;
        readonly TimeSpan _timeout = 12.Seconds();

        public SentMessageList()
        {
            _messages = new HashSet<ISentMessage>(new MessageIdEqualityComparer());
            _received = new AutoResetEvent(false);
        }

        public void Dispose()
        {
            using (_received)
            {
            }
        }

        public IEnumerator<ISentMessage> GetEnumerator()
        {
            lock (_messages)
                return _messages.ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Any()
        {
            return Any(x => true);
        }

        public bool Any<T>()
            where T : class
        {
            return Any(x => typeof(T).IsAssignableFrom(x.MessageType));
        }

        public bool Any<T>(Func<ISentMessage<T>, bool> filter) where T : class
        {
            return Any(x => typeof(T).IsAssignableFrom(x.MessageType) && filter((ISentMessage<T>)x));
        }

        public bool Any(Func<ISentMessage, bool> filter)
        {
            bool any;
            lock (_messages)
                any = _messages.Any(filter);

            while (any == false)
            {
                if (_received.WaitOne(_timeout, true) == false)
                    return false;

                lock (_messages)
                    any = _messages.Any(filter);
            }

            return true;
        }

        public void Add(ISentMessage message)
        {
            lock (_messages)
            {
                if (_messages.Add(message))
                    _received.Set();
            }
        }

        class MessageIdEqualityComparer :
            IEqualityComparer<ISentMessage>
        {
            public bool Equals(ISentMessage x, ISentMessage y)
            {
                return x.Equals(y);
            }

            public int GetHashCode(ISentMessage message)
            {
                return message.Context.GetHashCode();
            }
        }
    }
}
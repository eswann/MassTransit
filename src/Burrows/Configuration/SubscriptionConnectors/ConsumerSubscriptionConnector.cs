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

using System;
using Magnum.Extensions;
using Burrows.Pipeline;
using Burrows.Pipeline.Configuration;
using Burrows.Pipeline.Sinks;

namespace Burrows.Configuration.SubscriptionConnectors
{
    /// <summary>
    /// A connector for a specific message. Objects implementing this interface should be able to
    /// both do <see cref="IConsumerConnector"/> and be typed to a specific message.
    /// </summary>
    public interface IConsumerSubscriptionConnector :
        IConsumerConnector
    {
        Type MessageType { get; }
    }

    public class ConsumerSubscriptionConnector<TConsumer, TMessage> :
        IConsumerSubscriptionConnector
        where TConsumer : class, Consumes<TMessage>.All
        where TMessage : class
    {
        public Type MessageType
        {
            get { return typeof (TMessage); }
        }

        public UnsubscribeAction Connect<T>(IInboundPipelineConfigurator configurator, IConsumerFactory<T> factory)
            where T : class
        {
            var consumerFactory = factory as IConsumerFactory<TConsumer>;
            if (consumerFactory == null)
                throw new ArgumentException("The consumer factory is of an invalid type: " +
                                            typeof (T).ToShortTypeName());

            var sink = new ConsumerMessageSink<TConsumer, TMessage>(consumerFactory);

            return configurator.Pipeline.ConnectToRouter(sink, () => configurator.SubscribedTo<TMessage>());
        }
    }
}
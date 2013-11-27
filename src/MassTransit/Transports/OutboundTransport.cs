﻿// Copyright 2007-2012 Chris Patterson, Dru Sellers, Travis Smith, et. al.
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
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using Context;
    using Exceptions;
    using Magnum;
    using Publish;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Exceptions;

    public class OutboundTransport :
        IOutboundTransport
    {
        private readonly IRabbitMqEndpointAddress _address;
        private readonly IPublisherConfirmSettings _publisherConfirmSettings;
        private readonly IConnectionHandler<TransportConnection> _connectionHandler;
        Producer _producer;

        public OutboundTransport(IRabbitMqEndpointAddress address,
            IPublisherConfirmSettings publisherConfirmSettings,
            IConnectionHandler<TransportConnection> connectionHandler)
        {
            _address = address;
            _publisherConfirmSettings = publisherConfirmSettings;
            _connectionHandler = connectionHandler;
        }

        public IEndpointAddress Address
        {
            get { return _address; }
        }

        public void Send(ISendContext context)
        {
            AddProducerBinding();

            _connectionHandler.Use(connection =>
            {
                try
                {
                    IBasicProperties properties = _producer.CreateProperties();

                    properties.SetPersistent(true);
                    properties.MessageId = context.MessageId ?? properties.MessageId ?? NewId.Next().ToString();
                    if (context.ExpirationTime.HasValue)
                    {
                        DateTime value = context.ExpirationTime.Value;
                        properties.Expiration =
                            (value.Kind == DateTimeKind.Utc
                                 ? value - SystemUtil.UtcNow
                                 : value - SystemUtil.Now).
                                TotalMilliseconds.ToString("F0", CultureInfo.InvariantCulture);
                    }

                    using (var body = new MemoryStream())
                    {
                        context.SerializeTo(body);
                        properties.Headers = context.Headers.ToDictionary(entry => entry.Key, entry => (object)entry.Value);
                        properties.Headers["Content-Type"] = context.ContentType;

                        if (_publisherConfirmSettings.UsePublisherConfirms)
                        {
                            var task = _producer.PublishAsync(_address.Name, properties, body.ToArray());
                            task.Wait();
                        }
                        else
                        {
                            _producer.Publish(_address.Name, properties, body.ToArray());
                        }
                        _address.LogSent(context.MessageId ?? properties.MessageId ?? "", context.MessageType);
                    }
                }
                catch (AggregateException ex)
                {
                    throw new TransportException(_address.Uri, "Publisher did not confirm message", ex.InnerException);
                }
                catch (AlreadyClosedException ex)
                {
                    throw new InvalidConnectionException(_address.Uri, "Connection was already closed", ex);
                }
                catch (EndOfStreamException ex)
                {
                    throw new InvalidConnectionException(_address.Uri, "Connection was closed", ex);
                }
                catch (OperationInterruptedException ex)
                {
                    throw new InvalidConnectionException(_address.Uri, "Operation was interrupted", ex);
                }
            });
        }

        public void Dispose()
        {
            RemoveProducer();
        }

        void AddProducerBinding()
        {
            if (_producer != null)
                return;

            _producer = new Producer(_address, _publisherConfirmSettings);

            _connectionHandler.AddBinding(_producer);
        }

        void RemoveProducer()
        {
            if (_producer != null)
            {
                _connectionHandler.RemoveBinding(_producer);
            }
        }
    }
}
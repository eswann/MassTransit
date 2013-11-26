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
namespace MassTransit.Transports.RabbitMq
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Logging;
#if NET40
    using System.Threading.Tasks;
#endif
    using Magnum.Caching;
    using Magnum.Extensions;
    using Publish;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;


    public class RabbitMqProducer :
        IConnectionBinding<RabbitMqConnection>
    {
        readonly Cache<ulong, TaskCompletionSource<bool>> _confirms;
        static readonly ILog _log = Logger.Get<RabbitMqProducer>();
        readonly IRabbitMqEndpointAddress _address;
        readonly object _lock = new object();
        readonly IPublisherConfirmSettings _publisherConfirmSettings;

        IModel _channel;
        bool _immediate;
        bool _mandatory;

        public RabbitMqProducer(IRabbitMqEndpointAddress address, IPublisherConfirmSettings publisherConfirmSettings)
        {
            _address = address;
            _publisherConfirmSettings = publisherConfirmSettings;
            _confirms = new ConcurrentCache<ulong, TaskCompletionSource<bool>>();
        }

        public void Bind(RabbitMqConnection connection)
        {
            lock (_lock)
            {
                IModel channel = null;
                try
                {
                    channel = connection.Connection.CreateModel();

                    BindEvents(channel);

                    _channel = channel;
                }
                catch (Exception ex)
                {
                    channel.Cleanup(500, ex.Message);

                    throw new InvalidConnectionException(_address.Uri, "Invalid connection to host", ex);
                }
            }
        }

        void BindEvents(IModel channel)
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                channel.BasicAcks += HandleAck;
                channel.BasicNacks += HandleNack;
                channel.ConfirmSelect();
            }
        }

        public void Unbind(RabbitMqConnection connection)
        {
            lock (_lock)
            {
                try
                {
                    if (_channel != null)
                    {
                        if (_publisherConfirmSettings.UsePublisherConfirms)
                        {
                            WaitForPendingConfirms();
                        }
                        UnbindEvents(_channel);
                        _channel.Cleanup(200, "Producer Unbind");
                    }
                }
                finally
                {
                    if (_channel != null)
                        _channel.Dispose();
                    _channel = null;
                }
            }
        }

        void WaitForPendingConfirms()
        {
            try
            {
                bool timedOut;
                _channel.WaitForConfirms(60.Seconds(), out timedOut);
                if (timedOut)
                    _log.WarnFormat("Timeout waiting for all pending confirms on {0}", _address.Uri);
            }
            catch (Exception ex)
            {
                _log.Error("Waiting for pending confirms threw an exception", ex);
            }
        }

        void UnbindEvents(IModel channel)
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                channel.BasicAcks -= HandleAck;
                channel.BasicNacks -= HandleNack;
            }
        }

        public IBasicProperties CreateProperties()
        {
            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidConnectionException(_address.Uri, "Channel should not be null");

                return _channel.CreateBasicProperties();
            }
        }

        public void Publish(string exchangeName, IBasicProperties properties, byte[] body)
        {
            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidConnectionException(_address.Uri, "No connection to RabbitMQ Host");

                if (_publisherConfirmSettings.UsePublisherConfirms)
                {
                    var clientMessageId = (string)properties.Headers[PublisherConfirmSettings.ClientMessageId];

                    if (clientMessageId != null && _publisherConfirmSettings.RegisterMessageAction != null)
                    {
                        _publisherConfirmSettings.RegisterMessageAction(_channel.NextPublishSeqNo, clientMessageId);
                    }
                }

                _channel.BasicPublish(exchangeName, "", properties, body);
            }
        }


        void HandleNack(IModel model, BasicNackEventArgs args)
        {
            _publisherConfirmSettings.Nacktion(args.DeliveryTag, args.Multiple);
        }

        void HandleAck(IModel model, BasicAckEventArgs args)
        {
            _publisherConfirmSettings.Acktion(args.DeliveryTag, args.Multiple);
        }

    }
}
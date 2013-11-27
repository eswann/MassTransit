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
namespace MassTransit
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Exceptions;
    using Logging;
    using Magnum.Caching;
    using Magnum.Extensions;
    using RabbitMQ.Client;
    using RabbitMQ.Client.Events;
    using Transports;
    using Transports.Publish;

    public class Producer : IConnectionBinding<TransportConnection>
    {
        private readonly Cache<ulong, TaskCompletionSource<bool>> _confirms;
        private static readonly ILog _log = Logger.Get<Producer>();
        private readonly IEndpointAddress _address;
        private readonly IPublisherConfirmSettings _publisherConfirmSettings;
        private readonly object _lock = new object();
        IModel _channel;

        public Producer(IEndpointAddress address, IPublisherConfirmSettings publisherConfirmSettings)
        {
            _address = address;
            _publisherConfirmSettings = publisherConfirmSettings;

            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                _confirms = new ConcurrentCache<ulong, TaskCompletionSource<bool>>();
            }
        }

        public void Bind(TransportConnection connection)
        {
            lock (_lock)
            {
                IModel channel = null;
                try
                {
                    channel = connection.Connection.CreateModel();

                    BindEvents(channel);

                    if (_publisherConfirmSettings.UsePublisherConfirms)
                    {
                        channel.ConfirmSelect();
                    }

                    _channel = channel;
                }
                catch (Exception ex)
                {
                    if (channel != null)
                    {
                        channel.Cleanup(500, ex.Message);
                    }

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
            }
            //channel.BasicReturn += HandleReturn;
            //channel.FlowControl += HandleFlowControl;
            channel.ModelShutdown += HandleModelShutdown;
        }

        public void Unbind(TransportConnection connection)
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

                    FailPendingConfirms();
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
            //channel.BasicReturn -= HandleReturn;
            //channel.FlowControl -= HandleFlowControl;
            channel.ModelShutdown -= HandleModelShutdown;
        }

        void FailPendingConfirms()
        {
            try
            {
                var exception = new MessageNotConfirmedException(_address.Uri,
                    "Publish not confirmed before channel closed");

                _confirms.Each((id, task) => task.TrySetException(exception));
            }
            catch (Exception ex)
            {
                _log.Error("Exception while failing pending confirms", ex);
            }

            _confirms.Clear();
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

                _channel.BasicPublish(exchangeName, "", properties, body);
            }
        }

        public Task PublishAsync(string exchangeName, IBasicProperties properties, byte[] body)
        {
            lock (_lock)
            {
                if (_channel == null)
                    throw new InvalidConnectionException(_address.Uri, "No connection to RabbitMQ Host");

                ulong deliveryTag = _channel.NextPublishSeqNo;

                var task = new TaskCompletionSource<bool>();
                _confirms.Add(deliveryTag, task);

                try
                {
                    _channel.BasicPublish(exchangeName, "", properties, body);
                }
                catch
                {
                    _confirms.Remove(deliveryTag);
                    throw;
                }

                return task.Task;
            }
        }

        void HandleModelShutdown(IModel model, ShutdownEventArgs reason)
        {
            if (_publisherConfirmSettings.UsePublisherConfirms)
            {
                try
                {
                    FailPendingConfirms();
                }
                catch (Exception ex)
                {
                    _log.Error("Fail pending confirms failed during model shutdown", ex);
                }
            }
        }

        void HandleNack(IModel model, BasicNackEventArgs args)
        {
            IEnumerable<ulong> ids = Enumerable.Repeat(args.DeliveryTag, 1);
            if (args.Multiple)
                ids = _confirms.GetAllKeys().Where(x => x <= args.DeliveryTag);

            var exception = new InvalidOperationException("Publish was nacked by the broker");

            foreach (ulong id in ids)
            {
                _confirms[id].TrySetException(exception);
                _confirms.Remove(id);
            }
        }

        void HandleAck(IModel model, BasicAckEventArgs args)
        {
            IEnumerable<ulong> ids = Enumerable.Repeat(args.DeliveryTag, 1);
            if (args.Multiple)
                ids = _confirms.GetAllKeys().Where(x => x <= args.DeliveryTag);

            foreach (ulong id in ids)
            {
                _confirms[id].TrySetResult(true);
                _confirms.Remove(id);
            }
        }

        //void HandleFlowControl(IModel sender, FlowControlEventArgs args)
        //{
        //}

        //void HandleReturn(IModel model, BasicReturnEventArgs args)
        //{
        //}
    }
}
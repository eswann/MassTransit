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
    using Configuration.Extensions;
    using Logging;
    using Magnum.Extensions;
    using RabbitMQ.Client;

    public interface ITransportConnection : IDisposable
    {
        void Connect();
        void Disconnect();
    }

    public class TransportConnection : ITransportConnection
    {
        private static readonly ILog _log = Logger.Get(typeof (TransportConnection));
        private readonly ConnectionFactory _connectionFactory;
        RabbitMQ.Client.IConnection _connection;
        bool _disposed;

        public TransportConnection(ConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public RabbitMQ.Client.IConnection Connection
        {
            get { return _connection; }
        }

        public void Dispose()
        {
            if (_disposed)
                throw new ObjectDisposedException("RabbitMqConnection for {0}".FormatWith(_connectionFactory.GetUri()),
                    "Cannot dispose a connection twice");

            try
            {
                Disconnect();
            }
            finally
            {
                _disposed = true;
            }
        }

        public void Connect()
        {
            Disconnect();

            _connection = _connectionFactory.CreateConnection();
        }

        public void Disconnect()
        {
            _connection.Cleanup(200, "Disconnect");
            _connection = null;
        }
    }
}
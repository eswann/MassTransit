namespace MassTransit.Transports.RabbitMq.Publish
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    public class UnconfirmedMessageSqlRepository : IUnconfirmedMessageRepository
    {
        private readonly ILogger _logger = LoggingService.LoggerFor<UnconfirmedMessageSqlRepository>();
        private readonly IPublishSettings _publishSettings;
        private static readonly Assembly _messagesAssembly = typeof(IMessage).Assembly;
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new ConcurrentDictionary<string, Type>();

        public UnconfirmedMessageSqlRepository(IPublishSettings publishSettings)
        {
            _publishSettings = publishSettings;
        }

        public IEnumerable<IMessage> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            var results = new List<IMessage>();

            using (var dbConnection = new SqlConnection(_publishSettings.ConnectionString))
            {
                using (var command = dbConnection.CreateCommand())
                {
                    command.CommandText = "GetAndDeleteUnconfirmedMessages";
                    command.CommandType = CommandType.StoredProcedure;
                    
                    var param1 = command.Parameters.AddWithValue("@PublisherId", publisherId);
                    param1.DbType = DbType.String;
                    param1.Size = 50;
                    
                    var param2 = command.Parameters.AddWithValue("@PageSize", pageSize);
                    param2.DbType = DbType.Int32;
                    
                    dbConnection.Open();
                    using (var reader = command.ExecuteReader(CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            try
                            {
                                string messageTypeKey = reader.GetString(0);

                                Type messageType;
                                if (!_cachedTypes.TryGetValue(messageTypeKey, out messageType))
                                {
                                    messageType = _messagesAssembly.GetType(messageTypeKey, true);
                                    _cachedTypes.TryAdd(messageTypeKey, messageType);
                                }

                                var message = (IMessage)JsonConvert.DeserializeObject(reader.GetString(1), messageType);
                                results.Add(message);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error("The following error occurred while deserializing a Json object.", ex);
                            }
                        }
                    }
                }
            }
            return results;
        }

        public void StoreMessages(ConcurrentQueue<IMessage> messages, string publisherId)
        {
            var messagesToInsert = CreateInsertDataTable();

            IEnumerable<IMessage> messagesToProcess;

            bool hasMessages = GetNextMessageBatchFromQueue(messages, out messagesToProcess);

            while (hasMessages)
            {
                InsertMessages(publisherId, messagesToProcess, messagesToInsert);
                hasMessages = GetNextMessageBatchFromQueue(messages, out messagesToProcess);
            }
        }

        public void StoreMessages(IEnumerable<IMessage> messages, string publisherId)
        {
            int skip = 0;
            int take = _publishSettings.InsertStoredMessagesBatchSize;

            var messagesToInsert = CreateInsertDataTable();

            var messagesToProcess = messages.Skip(skip).Take(take);

            while (messagesToProcess.Any())
            {
                InsertMessages(publisherId, messagesToProcess, messagesToInsert);
                skip = skip + take;
                messagesToProcess = messages.Skip(skip).Take(take);
            }
        }

        private static DataTable CreateInsertDataTable()
        {
            var messagesToInsert = new DataTable("messagesToInsert");
            messagesToInsert.Columns.Add("MessageId", typeof (Guid));
            messagesToInsert.Columns.Add("MessageType", typeof(string));
            var column = messagesToInsert.Columns.Add("Message", typeof (string));
            column.MaxLength = 2000;
            return messagesToInsert;
        }

        private void InsertMessages(string publisherId, IEnumerable<IMessage> messagesToProcess, DataTable messagesToInsert)
        {
            foreach (var message in messagesToProcess)
            {
                messagesToInsert.Rows.Add(message.MessageId, message.GetType().FullName, JsonConvert.SerializeObject(message));
            }

            using (var dbConnection = new SqlConnection(_publishSettings.ConnectionString))
            {
                using (var command = dbConnection.CreateCommand())
                {
                    command.CommandText = "InsertUnconfirmedMessages";
                    command.CommandType = CommandType.StoredProcedure;

                    var param1 = command.Parameters.AddWithValue("@PublisherId", publisherId);
                    param1.DbType = DbType.String;
                    param1.Size = 50;

                    var param2 = command.Parameters.AddWithValue("@Messages", messagesToInsert);
                    param2.SqlDbType = SqlDbType.Structured;

                    dbConnection.Open();
                    command.ExecuteNonQuery();
                }
            }
            messagesToInsert.Clear();
        }

        private bool GetNextMessageBatchFromQueue(ConcurrentQueue<IMessage> concurrentQueue, out IEnumerable<IMessage> messagesToProcess)
        {
            var messages = new List<IMessage>(_publishSettings.InsertStoredMessagesBatchSize);

            IMessage message;
            int messageCount = 0;

            for (int i = 0; i < _publishSettings.InsertStoredMessagesBatchSize; i++)
            {
                if (concurrentQueue.TryDequeue(out message))
                {
                    messages.Add(message);
                }
                else
                {
                    messageCount = i;
                    break;
                }
            }
            messagesToProcess = messages;
            return messageCount > 0;
        }

    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;
using Burrows.Logging;
using Newtonsoft.Json;

namespace Burrows.PublisherConfirms.BackingStores
{
    public class UnconfirmedMessageSqlRepository : IUnconfirmedMessageRepository
    {
        private static readonly ILog _log = Logger.Get<UnconfirmedMessageSqlRepository>();
        private readonly PublishSettings _publishSettings;
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new ConcurrentDictionary<string, Type>();

        public UnconfirmedMessageSqlRepository(PublishSettings publishSettings)
        {
            _publishSettings = publishSettings;
        }

        public async Task<IEnumerable<ConfirmableMessage>> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            var results = new List<ConfirmableMessage>();

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
                    using (var reader = await command.ExecuteReaderAsync(CommandBehavior.CloseConnection))
                    {
                        while (await reader.ReadAsync())
                        {
                            try
                            {
                                string messageTypeKey = reader.GetString(0);

                                Type messageType;
                                if (!_cachedTypes.TryGetValue(messageTypeKey, out messageType))
                                {
                                    _cachedTypes.TryAdd(messageTypeKey, messageType);
                                }

                                var message = (ConfirmableMessage)JsonConvert.DeserializeObject(reader.GetString(1), messageType);
                                results.Add(message);
                            }
                            catch (Exception ex)
                            {
                                _log.Error("The following error occurred while deserializing a Json object.", ex);
                            }
                        }
                    }
                }
            }
            return results;
        }

        public async Task StoreMessages(ConcurrentQueue<ConfirmableMessage> messages, string publisherId)
        {
            var messagesToInsert = CreateInsertDataTable();

            IEnumerable<ConfirmableMessage> messagesToProcess;

            bool hasMessages = GetNextMessageBatchFromQueue(messages, out messagesToProcess);

            while (hasMessages)
            {
                await InsertMessages(publisherId, messagesToProcess, messagesToInsert);
                hasMessages = GetNextMessageBatchFromQueue(messages, out messagesToProcess);
            }
        }

        public async Task StoreMessages(IEnumerable<ConfirmableMessage> messages, string publisherId)
        {
            int skip = 0;
            int take = _publishSettings.InsertStoredMessagesBatchSize;

            var messagesToInsert = CreateInsertDataTable();

            var messagesToProcess = messages.Skip(skip).Take(take);

            while (messagesToProcess.Any())
            {
                await InsertMessages(publisherId, messagesToProcess, messagesToInsert);
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

        private async Task InsertMessages(string publisherId, IEnumerable<ConfirmableMessage> messagesToProcess, DataTable messagesToInsert)
        {
            foreach (var message in messagesToProcess)
            {
                messagesToInsert.Rows.Add(message.Id, message.GetType().FullName, JsonConvert.SerializeObject(message));
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
                    await command.ExecuteNonQueryAsync();
                }
            }
            messagesToInsert.Clear();
        }

        private bool GetNextMessageBatchFromQueue(ConcurrentQueue<ConfirmableMessage> concurrentQueue, out IEnumerable<ConfirmableMessage> messagesToProcess)
        {
            var messages = new List<ConfirmableMessage>(_publishSettings.InsertStoredMessagesBatchSize);

            ConfirmableMessage message;
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
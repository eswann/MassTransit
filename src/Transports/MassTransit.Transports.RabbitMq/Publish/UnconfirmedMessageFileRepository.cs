namespace MassTransit.Transports.RabbitMq.Publish
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Newtonsoft.Json;

    public class UnconfirmedMessageFileRepository : IUnconfirmedMessageRepository
    {
        private readonly ILogger _logger = LoggingService.LoggerFor<UnconfirmedMessageFileRepository>();

        private static readonly object _directoryLock = new object();
        private readonly string _filePath;
        private const char _messageSegmentDelimiter = '|';
        private readonly List<string> _existingDirectories = new List<string>();

        private static readonly Assembly _messagesAssembly = typeof(IMessage).Assembly;
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new ConcurrentDictionary<string, Type>();
        
        public UnconfirmedMessageFileRepository(IPublishSettings publishSettings)
        {
            _filePath = publishSettings.FileRepositoryPath;

            _filePath = Path.Combine(HttpRuntime.AppDomainId == null ? Directory.GetCurrentDirectory() : HttpRuntime.AppDomainAppPath, _filePath);

            if (!_filePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                _filePath += Path.DirectorySeparatorChar;
        }

        public IEnumerable<IMessage> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            var results = new List<IMessage>();
            var path = GetOrCreateDirectory(publisherId);
            var files = Directory.EnumerateFiles(path).OrderBy(x => x).Take(pageSize);

            foreach (var filePath in files)
            {
                string messageText;
                using (var streamReader = new StreamReader(filePath))
                {
                    messageText = streamReader.ReadToEnd();
                }

                string[] segments = messageText.Split(_messageSegmentDelimiter);

                try
                {
                    string messageTypeKey = segments[0];

                    Type messageType;
                    if (!_cachedTypes.TryGetValue(messageTypeKey, out messageType))
                    {
                        messageType = _messagesAssembly.GetType(messageTypeKey, true);
                        _cachedTypes.TryAdd(messageTypeKey, messageType);
                    }

                    var message = (IMessage)JsonConvert.DeserializeObject(segments[1], messageType);
                    results.Add(message);
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (FileNotFoundException)
                    {
                        _logger.Info("File deleted by another process.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("The following error occurred while deserializing a Json object.", ex);
                }
            }

            return results;
        }

        public void StoreMessages(ConcurrentQueue<IMessage> messages, string publisherId)
        {
            string path = GetOrCreateDirectory(publisherId);

            IMessage message;
            while (messages.TryDequeue(out message))
            {
                StoreMessage(message, path);
            }
        }

        public void StoreMessages(IEnumerable<IMessage> messages, string publisherId)
        {
            string path = GetOrCreateDirectory(publisherId);

            foreach (var message in messages)
            {
                StoreMessage(message, path);
            }
        }

        private async void StoreMessage(IMessage message, string path)
        {
            var messageText = message.GetType().FullName + _messageSegmentDelimiter + JsonConvert.SerializeObject(message);

            using (var outfile = new StreamWriter(path + Path.DirectorySeparatorChar + message.MessageId + ".txt"))
            {
                await outfile.WriteAsync(messageText);
            }
        }

        private string GetOrCreateDirectory(string publisherId)
        {
            string fullPath = _filePath + publisherId + Path.DirectorySeparatorChar;
            if (!_existingDirectories.Contains(fullPath))
            {
                lock (_directoryLock)
                {
                    if (!_existingDirectories.Contains(fullPath))
                    {
                        if (!Directory.Exists(fullPath))
                            Directory.CreateDirectory(fullPath);

                        _existingDirectories.Add(fullPath);
                    }
                }
            }
            return fullPath;
        }
    }
}
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Web;
using Burrows.Logging;
using Newtonsoft.Json;

namespace Burrows.BackedPublisher.BackingStores
{
    public class UnconfirmedMessageFileRepository : IUnconfirmedMessageRepository
    {
        private const char MessageSegmentDelimiter = '|';
        private static readonly ILog _log = Logger.Get <UnconfirmedMessageFileRepository>();

        private static readonly object _directoryLock = new object();
        private readonly string _filePath;
        
        private readonly List<string> _existingDirectories = new List<string>();

        //private static readonly Assembly _messagesAssembly = typeof(IMessage).Assembly;
        private readonly ConcurrentDictionary<string, Type> _cachedTypes = new ConcurrentDictionary<string, Type>();
        
        public UnconfirmedMessageFileRepository(PublishSettings publishSettings)
        {
            _filePath = publishSettings.FileRepositoryPath;

            _filePath = Path.Combine(HttpRuntime.AppDomainId == null ? Directory.GetCurrentDirectory() : HttpRuntime.AppDomainAppPath, _filePath);

            if (!_filePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                _filePath += Path.DirectorySeparatorChar;
        }

        public IEnumerable<Object> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            var results = new List<Object>();
            var path = GetOrCreateDirectory(publisherId);
            var files = Directory.EnumerateFiles(path).OrderBy(x => x).Take(pageSize);

            foreach (var filePath in files)
            {
                string messageText;
                using (var streamReader = new StreamReader(filePath))
                {
                    messageText = streamReader.ReadToEnd();
                }

                string[] segments = messageText.Split(MessageSegmentDelimiter);

                try
                {
                    string messageTypeKey = segments[0];

                    Type messageType;
                    if (!_cachedTypes.TryGetValue(messageTypeKey, out messageType))
                    {
                        //messageType = _messagesAssembly.GetType(messageTypeKey, true);
                        //_cachedTypes.TryAdd(messageTypeKey, messageType);
                    }

                    var message = JsonConvert.DeserializeObject(segments[1], messageType);
                    results.Add(message);
                    try
                    {
                        File.Delete(filePath);
                    }
                    catch (FileNotFoundException)
                    {
                        _log.Info("File deleted by another process.");
                    }
                }
                catch (Exception ex)
                {
                    _log.Error("The following error occurred while deserializing a Json object.", ex);
                }
            }

            return results;
        }

        public void StoreMessages(ConcurrentQueue<Object> messages, string publisherId)
        {
            string path = GetOrCreateDirectory(publisherId);

            Object message;
            while (messages.TryDequeue(out message))
            {
                StoreMessage(message, path);
            }
        }

        public void StoreMessages(IEnumerable<Object> messages, string publisherId)
        {
            string path = GetOrCreateDirectory(publisherId);

            foreach (var message in messages)
            {
                StoreMessage(message, path);
            }
        }

        private async void StoreMessage(Object message, string path)
        {
            var messageText = message.GetType().FullName + MessageSegmentDelimiter + JsonConvert.SerializeObject(message);

            //using (var outfile = new StreamWriter(path + Path.DirectorySeparatorChar + message.MessageId + ".txt"))
            //{
            //    await outfile.WriteAsync(messageText);
            //}
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
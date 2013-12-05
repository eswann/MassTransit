using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Burrows.Logging;
using Newtonsoft.Json;

namespace Burrows.PublisherConfirms.BackingStores
{
    public class UnconfirmedMessageFileRepository : IUnconfirmedMessageRepository
    {
        private static readonly ILog _log = Logger.Get<UnconfirmedMessageFileRepository>();

        private static readonly object _directoryLock = new object();
        private readonly string _rootFilePath;
        
        private readonly List<string> _existingDirectories = new List<string>();

        private readonly ConcurrentDictionary<string, long> _latestFileSequences = new ConcurrentDictionary<string, long>();
        
        public UnconfirmedMessageFileRepository(string rootFilePath)
        {
            _rootFilePath = Path.Combine(HttpRuntime.AppDomainId == null ? Directory.GetCurrentDirectory() : HttpRuntime.AppDomainAppPath, rootFilePath);

            if (!_rootFilePath.EndsWith(Path.DirectorySeparatorChar.ToString(CultureInfo.InvariantCulture)))
                _rootFilePath += Path.DirectorySeparatorChar;
        }

        public async Task<IList<ConfirmableMessage>> GetAndDeleteMessages(string publisherId, int pageSize)
        {
            var results = new List<ConfirmableMessage>();
            var path = GetOrCreateDirectory(publisherId);
            var files = Directory.EnumerateFiles(path).OrderBy(x => x).Take(pageSize);

            foreach (var filePath in files)
            {
                string messageText;
                using (var streamReader = new StreamReader(filePath))
                {
                    messageText = await streamReader.ReadToEndAsync();
                }

                try
                {
                    var message = JsonConvert.DeserializeObject<ConfirmableMessage>(messageText);
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

        public async Task StoreMessages(ConcurrentQueue<ConfirmableMessage> messages, string publisherId)
        {
            string path = GetOrCreateDirectory(publisherId);

            ConfirmableMessage message;
            while (messages.TryDequeue(out message))
            {
               await StoreMessage(message, path);
            }
        }

        private async Task StoreMessage(ConfirmableMessage message, string path)
        {
            string typeName = message.Type.FullName;

            var rootFilePath = path + typeName;

            var messageText = JsonConvert.SerializeObject(message);
            
            long ticks = DateTime.UtcNow.Ticks;

            long currentGreatestSequence;
            _latestFileSequences.TryGetValue(rootFilePath, out currentGreatestSequence);

            while (currentGreatestSequence >= ticks)
            {
                //Do nothing, just need to wait one tick.
                ticks = DateTime.UtcNow.Ticks;
            }
            _latestFileSequences[rootFilePath] = ticks;

            var filePath = rootFilePath + "_" + ticks + ".txt";

            using (var outfile = new StreamWriter(filePath))
            {
                await outfile.WriteAsync(messageText);
            }
        }

        private string GetOrCreateDirectory(string publisherId)
        {
            string fullPath = _rootFilePath + publisherId + Path.DirectorySeparatorChar;
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
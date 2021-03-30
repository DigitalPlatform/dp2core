using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using Nito.AsyncEx;
// using Microsoft.VisualStudio.Threading;

namespace DigitalPlatform.MessageQueue
{
    public class MessageQueue : IDisposable
    {
        string _databaseFileName = null;

        static QueueStorageCollection _storageTable = new QueueStorageCollection();

        QueueStorage _storage = null;

        // SemaphoreSlim _databaseLimit = new SemaphoreSlim(1);

        int _chunkSize = 4096;
        public int ChunkSize
        {
            get
            {
                return _chunkSize;
            }
            set
            {
                _chunkSize = value;
            }
        }

        // parameters:
        //      style   mode:Shared|Exclusive|ReadOnly
        public MessageQueue(string databaseFileName,
            string style = "")
        {
            _databaseFileName = databaseFileName;
            /*
            var mode = StringUtil.GetParameterByPrefix(style, "mode");
            if (string.IsNullOrEmpty(mode))
                mode = "Shared";

            this._mode = mode;
            */

            _storage = _storageTable.GetStorage(_databaseFileName);
        }

        public async Task PushAsync(List<string> texts,
            CancellationToken token = default)
        {
            DateTime now = DateTime.Now;
            foreach (string text in texts)
            {
                var message = new Message
                {
                    Content = Encoding.UTF8.GetBytes(text),
                    CreateTime = now,
                };

                using (MemoryStream ms = new MemoryStream())
                using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(datawriter, message);

                    await _storage.AppendAsync(ms.ToArray());
                }
            }
        }

#if REMOVED
        List<QueueItem> BuildItem(string text)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(text);
            return BuildItem(buffer);
        }

        List<QueueItem> BuildItem(byte[] buffer)
        {
            DateTime now = DateTime.Now;
            List<QueueItem> results = new List<QueueItem>();
            int start = 0;
            while (start < buffer.Length)
            {
                int length = Math.Min(buffer.Length - start, this.ChunkSize);
                byte[] fragment = new byte[length];
                Array.Copy(buffer, start, fragment, 0, length);
                QueueItem item = new QueueItem
                {
                    Content = fragment,
                    CreateTime = now
                };
                results.Add(item);

                start += length;
            }

            if (results.Count > 1)  // > 0
            {
                string group_id = Guid.NewGuid().ToString();
                foreach (var item in results)
                {
                    item.GroupID = group_id;
                }
            }

            return results;
        }
#endif
        public async Task PushAsync(List<byte[]> contents,
            CancellationToken token = default)
        {
            DateTime now = DateTime.Now;
            foreach (var content in contents)
            {
                var message = new Message
                {
                    Content = content,
                    CreateTime = now,
                };

                using (MemoryStream ms = new MemoryStream())
                using (BsonDataWriter datawriter = new BsonDataWriter(ms))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    serializer.Serialize(datawriter, message);

                    await _storage.AppendAsync(ms.ToArray());
                }
            }
        }

        public async Task<Message> PullAsync(CancellationToken token = default)
        {
            return await GetAsync(true, token).ConfigureAwait(false);
        }

        public long Count
        {
            get
            {
                return _storage.GetDataCount();
            }
        }

        public async Task<Message> GetAsync(bool remove_items,
            CancellationToken token = default)
        {
            var data = await _storage.GetAsync(remove_items);
            if (data == null)
                return null;

            using (MemoryStream ms = new MemoryStream(data))
            using (BsonDataReader reader = new BsonDataReader(ms))
            {
                JsonSerializer serializer = new JsonSerializer();
                return serializer.Deserialize<Message>(reader);
            }
        }

        public async Task<Message> PeekAsync(CancellationToken token = default)
        {
            return await GetAsync(false, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            if (_storage != null)
                _storageTable.ReturnStorage(_storage);
        }
    }

    public class Message
    {
        public byte[] Content { get; set; }
        public DateTime CreateTime { get; set; }

        public string GetString()
        {
            if (Content == null)
                return null;
            return Encoding.UTF8.GetString(Content);
        }
    }
}

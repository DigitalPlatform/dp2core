using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using LiteDB;
using Microsoft.VisualStudio.Threading;

namespace DigitalPlatform.SimpleMessageQueue
{
    public class MessageQueue : IDisposable
    {
        string _databaseFileName = null;

        LiteDatabase _database = null;

        AsyncSemaphore _databaseLimit = new AsyncSemaphore(1);

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

        public MessageQueue(string databaseFileName,
            bool ensureCreated = true)
        {
            _databaseFileName = databaseFileName;

            {
                _database = new LiteDatabase(_databaseFileName);

                var col = _database.GetCollection<QueueItem>("queue");

                // TODO: Id 字段用创建索引么？

                // https://stackoverflow.com/questions/49211472/do-you-use-ensureindex-only-once-or-for-evey-document-you-insert-into-the-dat
                col.EnsureIndex(x => x.GroupID);
            }
        }

        public async Task PushAsync(List<string> texts,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                var col = _database.GetCollection<QueueItem>("queue");
                foreach (string text in texts)
                {
                    col.InsertBulk(BuildItem(text));
                }
            }
        }

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

        public async Task Push(List<byte[]> contents,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {
                var col = _database.GetCollection<QueueItem>("queue");
                foreach (var content in contents)
                {
                    col.InsertBulk(BuildItem(content));
                }
            }
        }

        public async Task<Message> PullAsync(CancellationToken token = default)
        {
            return await Get(true, token);
        }

        public async Task<Message> Get(bool remove_items,
            CancellationToken token = default)
        {
            using (var releaser = await _databaseLimit.EnterAsync())
            {

                var col = _database.GetCollection<QueueItem>("queue");

                // TODO: Id 字段用创建索引么？

                // https://stackoverflow.com/questions/49211472/do-you-use-ensureindex-only-once-or-for-evey-document-you-insert-into-the-dat
                col.EnsureIndex(x => x.GroupID);

                List<QueueItem> items = new List<QueueItem>();

                var first = col.Query()
                    .OrderBy(x => x.Id)
                    .FirstOrDefault();
                if (first == null)
                    return null;

                items.Add(first);
                if (string.IsNullOrEmpty(first.GroupID))
                {
                    // 删除涉及到的事项
                    if (remove_items)
                    {
                        foreach (var item in items)
                        {
                            col.Delete(item.Id);
                        }
                    }
                    return new Message { Content = first.Content };
                }
                // 取出所有 GroupID 相同的事项，然后拼接起来
                var group_id = first.GroupID;
                List<byte> bytes = new List<byte>(first.Content);

                int id = first.Id;
                while (token.IsCancellationRequested == false)
                {
                    var current = col.Query().Where(o => o.Id > id).OrderBy(o => o.Id).FirstOrDefault();
                    if (current == null)
                        break;
                    if (current.GroupID != group_id)
                        break;
                    bytes.AddRange(current.Content);
                    id = current.Id;

                    items.Add(current);
                }

                // 删除涉及到的事项
                if (remove_items)
                {
                    foreach (var item in items)
                    {
                        col.Delete(item.Id);
                    }
                }

                return new Message { Content = bytes.ToArray() };
            }
        }

        public async Task<Message> PeekAsync(CancellationToken token = default)
        {
            return await Get(false, token);
        }

        public void Dispose()
        {
            _database?.Dispose();
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DigitalPlatform.Text;
using LiteDB;
using Nito.AsyncEx;
// using Microsoft.VisualStudio.Threading;

namespace DigitalPlatform.SimpleMessageQueue
{
    public class MessageQueue : IDisposable
    {
        string _databaseFileName = null;
        string _mode = "Shared";

        // LiteDatabase _database = null;

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
            var mode = StringUtil.GetParameterByPrefix(style, "mode");
            if (string.IsNullOrEmpty(mode))
                mode = "Shared";

            this._mode = mode;

            /*
            var connectionString = $"Filename={databaseFileName};Mode={mode}";

            {
                _database = new LiteDatabase(connectionString);

                var col = _database.GetCollection<QueueItem>("queue");

                // TODO: Id 字段用创建索引么？

                // https://stackoverflow.com/questions/49211472/do-you-use-ensureindex-only-once-or-for-evey-document-you-insert-into-the-dat
                col.EnsureIndex(x => x.GroupID);
            }
            */
        }

#if REMOVED
        // filename --> LiteDatabase
        static Hashtable _databaseTable = new Hashtable();

        LiteDatabase GetDatabase(bool ensure_index = true)
        {
            lock (_databaseTable.SyncRoot)
            {
                LiteDatabase database = (LiteDatabase)_databaseTable[this._databaseFileName];
                if (database != null)
                {
                    /*
                    var ret = database.BeginTrans();
                    if (ret == false)
                        throw new Exception("BeginTrans error");
                    */
                    return database;
                }

                var connectionString = $"Filename={_databaseFileName};Mode={_mode}";
                database = new LiteDatabase(connectionString);
                if (ensure_index)
                {
                    var col = database.GetCollection<QueueItem>("queue");
                    col.EnsureIndex(x => x.Id);
                    // https://stackoverflow.com/questions/49211472/do-you-use-ensureindex-only-once-or-for-evey-document-you-insert-into-the-dat
                    col.EnsureIndex(x => x.GroupID);
                }
                _databaseTable[_databaseFileName] = database;
                /*
                {
                    var ret = database.BeginTrans();
                    if (ret == false)
                        throw new Exception("BeginTrans error");
                }
                */
                return database;
            }
        }

#endif

        LiteDatabase GetDatabase(bool ensure_index = true)
        {
            var connectionString = $"Filename={_databaseFileName};Mode={_mode}";
            var database = new LiteDatabase(connectionString);
            if (ensure_index)
            {
                var col = database.GetCollection<QueueItem>("queue");
                col.EnsureIndex(x => x.Id);
                // https://stackoverflow.com/questions/49211472/do-you-use-ensureindex-only-once-or-for-evey-document-you-insert-into-the-dat
                col.EnsureIndex(x => x.GroupID);
            }
            return database;
        }

        void ReturnDatabase(LiteDatabase database)
        {
            database.Dispose();
        }

        /*
        void DisposeAllDatabase()
        {
            if (_databaseTable != null)
            {
                lock (_databaseTable.SyncRoot)
                {
                    foreach (var key in _databaseTable.Keys)
                    {
                        LiteDatabase database = (LiteDatabase)_databaseTable[key];
                        database.Dispose();
                    }

                    _databaseTable.Clear();
                }
            }
        }
        */

        public async Task PushAsync(List<string> texts,
            CancellationToken token = default)
        {
            lock (_syncRoot)
            {
                var database = GetDatabase();
                try
                {

                    var col = database.GetCollection<QueueItem>("queue");
                    foreach (string text in texts)
                    {
                        col.InsertBulk(BuildItem(text));
                    }
                }
                finally
                {
                    ReturnDatabase(database);
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

        public async Task PushAsync(List<byte[]> contents,
            CancellationToken token = default)
        {
            lock (_syncRoot)
            {
                var database = GetDatabase();
                try
                {

                    var col = database.GetCollection<QueueItem>("queue");
                    foreach (var content in contents)
                    {
                        col.InsertBulk(BuildItem(content));
                    }

                    // col.EnsureIndex(x => x.Id);
                }
                finally
                {
                    ReturnDatabase(database);
                }
            }
        }

        public async Task<Message> PullAsync(CancellationToken token = default)
        {
            return await GetAsync(true, token).ConfigureAwait(false);
        }

        object _syncRoot = new object();

        public int Count
        {
            get
            {
                var database = GetDatabase();
                try
                {
                    lock (database)
                    {
                        var col = database.GetCollection<QueueItem>("queue");
                        return col.Count();
                    }
                }
                finally
                {
                    ReturnDatabase(database);
                }
            }
        }

        public async Task<Message> GetAsync(bool remove_items,
            CancellationToken token = default)
        {
            lock (_syncRoot)
            {
                var database = GetDatabase();
                try
                {

                    var col = database.GetCollection<QueueItem>("queue");

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
                finally
                {
                    ReturnDatabase(database);
                }
            }
        }

        public async Task<Message> PeekAsync(CancellationToken token = default)
        {
            return await GetAsync(false, token).ConfigureAwait(false);
        }

        public void Dispose()
        {
            // DisposeAllDatabase();
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

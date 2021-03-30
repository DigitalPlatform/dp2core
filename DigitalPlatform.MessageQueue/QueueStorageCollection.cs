using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageQueue
{
    /// <summary>
    /// 管理 QueueStorage 的集合。共用 Storage 的机制
    /// </summary>
    public class QueueStorageCollection : IDisposable
    {
        Hashtable _collection = new Hashtable();

        public void Dispose()
        {
            lock (_collection.SyncRoot)
            {
                foreach (var key in _collection.Keys)
                {
                    var storage = (QueueStorage)_collection[key];
                    if (storage != null)
                        storage.Dispose();
                }
                _collection.Clear();
            }
        }

        // 征用
        public QueueStorage GetStorage(string filename)
        {
            lock(_collection.SyncRoot)
            {
                var storage = (QueueStorage)_collection[filename];
                if (storage != null)
                {
                    storage.RefCount++;
                    return storage;
                }
                storage = new QueueStorage(filename);
                _collection[filename] = storage;
                storage.RefCount++;
                return storage;
            }
        }

        // 归还
        public void ReturnStorage(QueueStorage storage)
        {
            lock (_collection.SyncRoot)
            {
                storage.RefCount--;

                // 如果再也没有人占用了，则释放 QueueStorage
                if (storage.RefCount == 0)
                {
                    foreach (var key in _collection.Keys)
                    {
                        var current = (QueueStorage)_collection[key];
                        if (current == storage)
                        {
                            _collection.Remove(key);
                            current.Dispose();
                            return;
                        }
                    }
                }
            }
        }
    }
}

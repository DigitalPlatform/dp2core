using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.Messaging;

namespace UnitTestSimpleMessageQueue
{
    [TestClass]
    public class TestQueueStorage
    {
        // 测试 Append() 和 Get()
        [TestMethod]
        public async Task Test_append_and_get_1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            using var storage = new QueueStorage(fileName);
            var bytes = Encoding.UTF8.GetBytes("测试内容");
            await storage.AppendAsync(bytes);

            {
                var data = await storage.GetAsync(false);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }

            {
                var data = await storage.GetAsync(false);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }
        }

        // 测试 Append() 和 Get()
        [TestMethod]
        public async Task Test_append_and_get_2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            using var storage = new QueueStorage(fileName);
            var bytes = Encoding.UTF8.GetBytes("测试内容");
            await storage.AppendAsync(bytes);

            {
                // 获取并移走。这样数据就被取完了
                var data = await storage.GetAsync(true);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }

            {
                // 此时就没有数据了
                var data = await storage.GetAsync(true);

                Assert.AreEqual(null, data);
            }
        }

        // 测试 Append() 直到产生两个数据文件
        [TestMethod]
        public async Task Test_create_two_files()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            using var storage = new QueueStorage(fileName);
            storage.ChunkSize = 4096;
            byte[] buffer = new byte[1024];
            Array.Clear(buffer, 0, buffer.Length);
            // 出现第二个数据文件时的块编号
            int index = -1;
            for (int i = 0; ; i++)
            {
                await storage.AppendAsync(buffer);
                long count = storage.GetFileCount();
                if (count > 1)
                {
                    index = i;
                    break;
                }
            }

            Assert.AreNotEqual(-1, index);

            // 把数据全部取出
            for (int i = 0; ; i++)
            {
                var result = await storage.GetAsync(true);
                if (result == null)
                    break;
                long count = storage.GetFileCount();
                if (count == 1)
                {
                    Assert.AreEqual(index, i);
                }
            }

            {
                long count = storage.GetDataCount();
                Assert.AreEqual(0, count);
            }
        }

        // 确认取空的文件确实被删除了
        [TestMethod]
        public async Task Test_create_two_files_2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            {
                using var storage = new QueueStorage(fileName);
                storage.ChunkSize = 4096;
                byte[] buffer = new byte[1024];
                Array.Clear(buffer, 0, buffer.Length);
                // 出现第二个数据文件时的块编号
                int index = -1;
                for (int i = 0; ; i++)
                {
                    await storage.AppendAsync(buffer);
                    long count = storage.GetFileCount();
                    if (count > 1)
                    {
                        index = i;
                        break;
                    }
                }

                Assert.AreNotEqual(-1, index);

                // 把数据取出，直到只剩下一个文件
                for (int i = 0; ; i++)
                {
                    var result = await storage.GetAsync(true);
                    if (result == null)
                        break;
                    long count = storage.GetFileCount();
                    if (count == 1)
                    {
                        Assert.AreEqual(index, i);
                        break;
                    }
                }
            }

            // 重新打开，检查文件数
            {
                using var storage = new QueueStorage(fileName);

                long file_count = storage.GetFileCount();
                Assert.AreEqual(1, file_count);
            }
        }

        // 测试 .Clear()
        [TestMethod]
        public async Task Test_clear_files()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            // 创建多个数据文件
            {
                using var storage = new QueueStorage(fileName);
                storage.ChunkSize = 4096;
                byte[] buffer = new byte[1024];
                Array.Clear(buffer, 0, buffer.Length);
                for (int i = 0; ; i++)
                {
                    await storage.AppendAsync(buffer);
                    long count = storage.GetFileCount();
                    if (count >= 3)
                        break;
                }

            }

            // 重新打开，检查文件数
            {
                using var storage = new QueueStorage(fileName);

                long file_count = storage.GetFileCount();
                Assert.AreEqual(3, file_count);

                // 清除所有数据文件
                storage.Clear();
            }

            // 重新打开，检查文件数
            {
                using var storage = new QueueStorage(fileName);

                long file_count = storage.GetFileCount();
                Assert.AreEqual(0, file_count);
            }
        }

        // 测试写入和读出的顺序
        [TestMethod]
        public async Task Test_sequence_1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            int data_count = 0;
            // 创建多个数据文件
            {
                using var storage = new QueueStorage(fileName);
                storage.ChunkSize = 4096;
                for (int i = 0; ; i++)
                {
                    byte[] buffer = Encoding.UTF8.GetBytes($"这是数据块{i}");
                    await storage.AppendAsync(buffer);
                    data_count++;
                    long file_count = storage.GetFileCount();
                    if (file_count >= 10)
                        break;
                }
            }

            // 重新打开，检查文件数
            {
                using var storage = new QueueStorage(fileName);

                long file_count = storage.GetFileCount();
                Assert.AreEqual(10, file_count);

                // 检查数据记录个数
                Assert.AreEqual(data_count, storage.GetDataCount());

                // 读出，检查顺序
                int i = 0;
                for (i = 0; ; i++)
                {
                    var bytes = await storage.GetAsync(true);
                    if (bytes == null)
                        break;
                    string text = Encoding.UTF8.GetString(bytes);
                    Assert.AreEqual($"这是数据块{i}", text);
                }

                Assert.AreEqual(data_count, i);
            }
        }

    }
}



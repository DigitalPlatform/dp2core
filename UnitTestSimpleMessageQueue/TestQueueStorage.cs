using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.MessageQueue;

namespace UnitTestSimpleMessageQueue
{
    [TestClass]
    public class TestQueueStorage
    {
        // 最简单的冒烟测试
        [TestMethod]
        public void Test_append_and_get_1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            using var storage = new QueueStorage(fileName);
            var bytes = Encoding.UTF8.GetBytes("测试内容");
            storage.Append(bytes);

            {
                var data = storage.Get(false);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }

            {
                var data = storage.Get(false);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }
        }

        [TestMethod]
        public void Test_append_and_get_2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "queue_storage");
            QueueStorage.DeleteDataFiles(fileName);

            using var storage = new QueueStorage(fileName);
            var bytes = Encoding.UTF8.GetBytes("测试内容");
            storage.Append(bytes);

            {
                var data = storage.Get(true);

                Assert.AreNotEqual(null, data);

                string text = Encoding.UTF8.GetString(data);

                Assert.AreEqual("测试内容", text);
            }

            {
                var data = storage.Get(true);

                Assert.AreEqual(null, data);
            }
        }


    }
}



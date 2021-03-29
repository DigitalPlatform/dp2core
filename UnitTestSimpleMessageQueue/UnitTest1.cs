using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using DigitalPlatform.SimpleMessageQueue;

namespace UnitTestSimpleMessageQueue
{
    [TestClass]
    public class UnitTest1
    {
        // ��򵥵�ð�̲���
        [TestMethod]
        public async Task TestMethod1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            await queue.PushAsync(new List<string> { "1" });

            var message = await queue.PullAsync();
            Assert.AreEqual("1", message.GetString());

            message = await queue.PullAsync();
            Assert.AreEqual(null, message);
        }

        // ѭ�� Push �� Pull
        [TestMethod]
        public async Task TestMethod2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            for (int i = 0; i < 100; i++)
            {
                await queue.PushAsync(new List<string> { $"{i + 1}" });
            }

            for (int i = 0; i < 100; i++)
            {
                var message = await queue.PullAsync();
                Assert.AreEqual(message.GetString(), $"{i + 1}");
            }

            {
                var message = await queue.PullAsync();
                Assert.AreEqual(message, null);
            }
        }

        // ��ߴ����Ϣ����
        [TestMethod]
        public async Task TestMethod3()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);
            queue.ChunkSize = 4096;

            List<byte> bytes = new List<byte>();
            for (int i = 0; i < queue.ChunkSize * 20; i++)
            {
                bytes.Add((byte)i);
            }
            await queue.PushAsync(new List<byte[]> { bytes.ToArray() });

            var message = await queue.PullAsync();

            Assert.AreEqual(message.Content.Length, bytes.Count);
            for (int i = 0; i < queue.ChunkSize * 20; i++)
            {
                Assert.AreEqual(bytes[i], message.Content[i]);
            }

            message = await queue.PullAsync();
            Assert.AreEqual(message, null);
        }

        // TODO: CancellationToken �����ж�Ҫ����

        // ��ȡ�ն���(��ȡǰ�ļ�δ����)
        [TestMethod]
        public async Task Test_read_empty()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            var message = await queue.PullAsync();
            Assert.AreEqual(null, message);
        }

        // ��д��һ�������ȡ
        [TestMethod]
        public async Task Test_batch_write_and_read_1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            for (int i = 0; i < 100; i++)
            {
                await queue.PushAsync(new List<string> { i.ToString() });
            }

            for (int i = 0; i < 100; i++)
            {
                var message = await queue.PullAsync();
                Assert.AreEqual(i.ToString(), message.GetString());
            }

            {
                var message = await queue.PullAsync();
                Assert.AreEqual(null, message);
            }

        }

        // �����̣߳�ͬʱд��Ͷ�ȡ��д������
        [TestMethod]
        public async Task Test_batch_write_and_read_2()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            var task1 = Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    await queue.PushAsync(new List<string> { i.ToString() });
                }
            });


            var task2 = Task.Run(async () =>
            {
                for (int i = 0; i < 100;)
                {
                    var message = await queue.PullAsync();
                    if (message != null)
                    {
                        Assert.AreEqual(i.ToString(), message.GetString());
                        i++;
                    }
                }
            });

            await Task.WhenAll(task1, task2);

            {
                var message = await queue.PullAsync();
                Assert.AreEqual(0, queue.Count);
                Assert.AreEqual(null, message);
            }

        }


        // �����̣߳�ͬʱд��Ͷ�ȡ��д�����
        [TestMethod]
        public async Task Test_batch_write_and_read_3()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            using MessageQueue queue = new MessageQueue(fileName);

            var task1 = Task.Run(async () =>
            {
                for (int i = 0; i < 100; i++)
                {
                    await queue.PushAsync(new List<string> { i.ToString() });
                }
            });


            var task2 = Task.Run(async () =>
            {
                for (int i = 0; i < 100;)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    var message = await queue.PullAsync();
                    if (message != null)
                    {
                        Assert.AreEqual(i.ToString(), message.GetString());
                        i++;
                    }
                }
            });

            await Task.WhenAll(task1, task2);

            {
                var message = await queue.PullAsync();
                Assert.AreEqual(null, message);
            }

        }


        // �����̣߳����Դ���һ�� MessageQueue ����ͬʱд��Ͷ�ȡ
        [TestMethod]
        public async Task Test_batch_write_and_read_4()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            var task1 = Task.Run(async () =>
            {
                using (MessageQueue queue = new MessageQueue(fileName, "mode:Shared"))
                {
                    for (int i = 0; i < 100; i++)
                    {
                        await queue.PushAsync(new List<string> { i.ToString() });
                    }
                }
            });


            var task2 = Task.Run(async () =>
            {
                // �ڶ����߳��Գ�һ��������������������״̬
                // await Task.Delay(TimeSpan.FromSeconds(1));
                using (MessageQueue queue = new MessageQueue(fileName, "mode:Shared"))
                {

                    for (int i = 0; i < 100;)
                    {
                        await Task.Delay(TimeSpan.FromMilliseconds(10));
                        var message = await queue.PullAsync();
                        if (message != null)
                        {
                            Assert.AreEqual(i.ToString(), message.GetString());
                            i++;
                        }
                    }
                }
            });

            await Task.WhenAll(task1, task2);

            {
                using MessageQueue queue = new MessageQueue(fileName, "mode:ReadOnly");
                var message = await queue.PullAsync();
                Assert.AreEqual(null, message);
            }

        }


        [TestMethod]
        public async Task Test_shared_open_1()
        {
            string fileName = Path.Combine(Environment.CurrentDirectory, "mq.db");
            File.Delete(fileName);

            MessageQueue queue1 = new MessageQueue(fileName, "mode:Shared");
            {
                for (int i = 0; i < 100; i++)
                {
                    await queue1.PushAsync(new List<string> { i.ToString() });
                }
            }


            MessageQueue queue2 = new MessageQueue(fileName, "mode:Shared");
            {

                for (int i = 0; i < 100;)
                {
                    await Task.Delay(TimeSpan.FromMilliseconds(10));
                    var message = await queue2.PullAsync();
                    if (message != null)
                    {
                        Assert.AreEqual(i.ToString(), message.GetString());
                        i++;
                    }
                }
            }

            queue1.Dispose();
            queue2.Dispose();
        }

    }
}

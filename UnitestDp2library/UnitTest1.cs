using DigitalPlatform.IO;
using DigitalPlatform.LibraryServer;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text;

namespace UnitestDp2library
{
    [TestClass]
    public class UnitTestLibraryApplication
    {
        [TestMethod]
        public void Test_CompressDirectory_1()
        {
            string strBase = Environment.CurrentDirectory;
            string strDirectory = Path.Combine(Environment.CurrentDirectory,
                "testcompress_source");
            string strZipFileName = Path.Combine(Environment.CurrentDirectory,
                "test.zip");

            // 创建源目录
            {
                PathUtil.DeleteDirectory(strDirectory);

                /*
                 * file1
                 * directory1\
                 * directory1\file2
                 * */
                Directory.CreateDirectory(strDirectory);
                File.WriteAllText(Path.Combine(strDirectory, "file1"), "file1_content");

                string sub_dir1 = Path.Combine(strDirectory, "directory1");
                Directory.CreateDirectory(sub_dir1);

                File.WriteAllText(Path.Combine(sub_dir1, "file2"), "file2_content");
            }

            List<string> filenames = new List<string>();
            filenames.Add("testcompress_source\\file1");
            filenames.Add("testcompress_source\\directory1\\file2");

            int nRet = LibraryApplication.CompressDirectory(
    strDirectory,
    strBase,
    strZipFileName,
    Encoding.UTF8,
    false,
    out string strError);
            Assert.AreEqual(2, nRet);

            // 检查压缩文件里每个事项的文件名是否正确
            using (var zip = ZipFile.OpenRead(strZipFileName))
            {
                foreach (var entry in zip.Entries)
                {
                    var path = entry.FullName;
                    filenames.Remove(path);
                }
            }

            Assert.AreEqual(0, filenames.Count);

            // 检查文件内容是否完全一致
            // 检查文件时间是否完全一致
            using (var zip = ZipFile.OpenRead(strZipFileName))
            {
                foreach (var entry in zip.Entries)
                {
                    var source_filepath = Path.Combine(strBase, entry.FullName);
                    var source_lasttime = File.GetLastWriteTime(source_filepath);

                    // Assert.AreEqual(source_lasttime.Ticks, entry.LastWriteTime.Ticks);

                    using var source_stream = File.Open(source_filepath, FileMode.Open);
                    using var stream = entry.Open();
                    Assert.AreEqual(entry.Length, source_stream.Length);
                    Assert.AreEqual(true, StreamEquals(source_stream, stream));
                }
            }

            // 解压
            string strTargetDirectory = Path.Combine(Environment.CurrentDirectory,
    "testextract_target");

            PathUtil.DeleteDirectory(strTargetDirectory);

            ZipFile.ExtractToDirectory(strZipFileName, strTargetDirectory);

            // 比较解压后的文件的最后修改时间，和压缩前的原始文件
            {
                List<string> names = new List<string>();
                names.Add("file1");
                names.Add("directory1\\file2");

                foreach (var name in names)
                {
                    string origin_path = Path.Combine(strDirectory, name);
                    Assert.AreEqual(true, File.Exists(origin_path));

                    string target_path = Path.Combine(strTargetDirectory, "testcompress_source\\" + name);
                    Assert.AreEqual(true, File.Exists(target_path));

                    var origin_time = File.GetLastWriteTime(origin_path);
                    var target_time = File.GetLastWriteTime(target_path);

                    Assert.AreEqual(origin_time.ToString("yyyyMMdd_HHmmss_ff"), target_time.ToString("yyyyMMdd_HHmmss_ff"));
                }

            }
        }

        private bool StreamEquals(Stream a, Stream b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null)
            {
                throw new ArgumentNullException(a == null ? "a" : "b");
            }

            /*
            if (a.Length != b.Length)
            {
                return false;
            }
            */

            for (int i = 0; i < a.Length; i++)
            {
                int aByte = a.ReadByte();
                int bByte = b.ReadByte();
                if (aByte.CompareTo(bByte) != 0)
                {
                    return false;
                }
            }

            return true;
        }
    }
}

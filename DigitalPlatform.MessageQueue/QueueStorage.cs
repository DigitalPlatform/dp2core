using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DigitalPlatform.MessageQueue
{
    /// <summary>
    /// 队列的磁盘存储机制
    /// </summary>
    public class QueueStorage : IDisposable
    {
        string _prefix = null;

        // 一个分块文件的推荐尺寸
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

        // 索引文件
        // FileItem _indexFile = null;

        // 数据文件
        List<FileItem> _dataFiles = new List<FileItem>();


        public QueueStorage(string prefix)
        {
            OpenExisting(prefix);

            // CreateNew(prefix);
        }

        public void Close()
        {
            // _indexFile?.Close();

            if (_dataFiles != null)
            {
                foreach (var file in _dataFiles)
                {
                    file?.Close();
                }
                _dataFiles.Clear();
            }
        }

        // 删除和一个前缀有关的全部数据文件
        public static int DeleteDataFiles(string prefix)
        {
            int count = 0;

            string directory = Path.GetDirectoryName(prefix);
            string filename_prefix = Path.GetFileName(prefix);
            var di = new DirectoryInfo(directory);
            var fis = di.GetFiles($"{filename_prefix}_*.data");

            foreach (var fi in fis)
            {
                File.Delete(fi.FullName);
                count++;
            }

            return count;
        }

        void OpenExisting(string prefix)
        {
            _prefix = prefix;
            // _indexFile = FileItem.Open(prefix + ".index");
            _dataFiles = InitializeDataFiles(prefix/*,
                _indexFile.Stream*/);
        }

        /*
        void CreateNew(string prefix)
        {
            _prefix = prefix;
            // _indexFile = FileItem.Create(prefix + ".index");
            _dataFiles = new List<FileItem>();
        }
        */

        static string GetNumber(string filename)
        {
            int start = filename.IndexOf("_");
            if (start == -1)
                return null;
            string number = filename.Substring(start + 1);
            int end = number.IndexOf(".");
            if (end == -1)
                return null;
            return number.Substring(0, end);
        }

        // TODO: 优化为只打开第一个和最尾一个文件，中间的不打开
        static List<FileItem> InitializeDataFiles(string prefix)
        {
            // 搜集文件名中的编号
            string directory = Path.GetDirectoryName(prefix);
            string filename_prefix = Path.GetFileName(prefix);
            var di = new DirectoryInfo(directory);
            var fis = di.GetFiles($"{filename_prefix}_*.data");

            List<int> numbers = new List<int>();
            foreach (var fi in fis)
            {
                string number = GetNumber(fi.Name);
                if (number == null)
                    continue;
                if (Int32.TryParse(number, out int value) == false)
                    continue;
                numbers.Add(value);
            }

            // 按照数值大小排序
            numbers.Sort();

            List<FileItem> results = new List<FileItem>();

            foreach (var value in numbers)
            {
                string filename = $"{prefix}_{value.ToString()}.data";
                var item = FileItem.Open(filename);
                results.Add(item);
            }

            return results;
        }

#if REMOVED
        // TODO: 优化为只打开第一个和最尾一个文件，中间的不打开
        static List<FileItem> InitializeDataFiles(string prefix/*, 
            Stream indexStream*/)
        {
            List<FileItem> results = new List<FileItem>();
            indexStream.Seek(0, SeekOrigin.Begin);
            int count = 0;
            for(; ; )
            {
                byte[] buffer = new byte[4];
                var ret = indexStream.Read(buffer);
                if (ret < 4)
                    break;
                int value = BitConverter.ToInt32(buffer);
                if (value == -1)
                {
                    count++;
                    continue;
                }

                string filename = $"{prefix}_{value.ToString()}.data";
                var item = FileItem.Open(filename);
                results.Add(item);
                count++;
            }

            // 截断文件
            indexStream.SetLength(count * 4);
            return results;
        }

#endif

        // 新增一个数据文件
        FileItem NewDataFile()
        {
            int number = 1;
            if (_dataFiles.Count > 0)
            {
                var tailFile = _dataFiles[_dataFiles.Count - 1];
                string tailNumber = GetNumber(Path.GetFileName(tailFile.FileName));
                if (Int32.TryParse(tailNumber, out int value) == false)
                    throw new Exception($"文件名 '{tailFile.FileName}' 格式错误");
                number = value + 1;
            }

            var item = FileItem.Create($"{_prefix}_{number}.data");
            _dataFiles.Add(item);
            return item;
        }

        // 追加一个数据单元
        public void Append(byte[] data)
        {
            FileItem tailFile = null;
            // 找到最后一个数据文件
            if (_dataFiles.Count > 0)
            {
                tailFile = _dataFiles[_dataFiles.Count - 1];

                // 观察这个数据文件的大小。如果追加后会超过极限，则新增一个文件
                if (tailFile.Stream.Length > 0
                    && tailFile.Stream.Length + data.Length + 5 > _chunkSize)
                    tailFile = NewDataFile();
            }
            else
                tailFile = NewDataFile();

            AppendData(tailFile.Stream, data);
        }

        static void AppendData(Stream stream, byte[] data)
        {
            stream.Seek(0, SeekOrigin.End);

            // 写入长度 4 bytes
            byte[] buffer = BitConverter.GetBytes((Int32)(data.Length + 5));
            // Array.Clear(buffer, 0, buffer.Length);
            stream.Write(buffer);
            // 写入状态 byte。1 表示有内容
            stream.WriteByte(1);
            // 写入数据
            stream.Write(data);
        }

        // 获得并移走一个数据单元
        public byte[] Get(bool remove)
        {
        REDO:
            if (_dataFiles.Count == 0)
                return null;    // 表示没有任何数据了
            // 找到第一个数据文件
            FileItem firstFile = null;
            firstFile = _dataFiles[0];


            var data = GetFirstData(firstFile.Stream, out long offset);
            if (data == null)
            {
                // 文件全是空的
                _dataFiles.RemoveAt(0);
                firstFile.Delete();
                goto REDO;
            }

            // 从中移走数据。如果文件空了，要删除这个文件
            if (remove)
                MaskBlankData(firstFile.Stream, offset);

            // 如果必要，还需要收缩索引文件空闲空间

            return data;
        }

        // 定位第一个数据单元
        static byte[] GetFirstData(Stream stream, out long offset)
        {
            offset = -1;
            stream.Seek(0, SeekOrigin.Begin);
            while (true)
            {
                // 读出长度 4 bytes
                byte[] buffer = new byte[4];
                var ret = stream.Read(buffer);
                if (ret < 4)
                    break;
                Int32 length = BitConverter.ToInt32(buffer);

                // 读出状态 byte
                int value = stream.ReadByte();
                if (value == -1)
                    break;
                // 空块
                if (value == 0)
                {
                    // 跳过当前块，到下一块
                    stream.Seek(length - 5, SeekOrigin.Current);
                    continue;
                }

                // 读出数据
                offset = stream.Position - 5;

                byte[] data = new byte[length - 5];
                int read_length = stream.Read(data);
                if (read_length < data.Length)
                    throw new Exception($"希望读出 {data.Length}，但只读出了 {read_length}");

                return data;
            }

            return null;    // 全部都是空块
        }

        // 标记空白块
        static void MaskBlankData(Stream stream, long offset)
        {
#if DEBUG
            if (stream.Length <= offset)
                throw new Exception($"offset={offset} 越过文件末尾 {stream.Length}");
#endif
            stream.Seek(offset + 4, SeekOrigin.Begin);
            stream.WriteByte(0);
        }

        // 取得第一个数据单元的内容，但并不移走它
        public byte[] Peek()
        {
            return Get(false);
        }

        public void Dispose()
        {
            this.Close();
        }

        // 数据文件的结构是变长结构。每一块的头部有本块长度，和一个标志位。
        // 通过标志位能看出本块是否空闲
    }

    public class FileItem
    {
        public string FileName { get; set; }
        public Stream Stream { get; set; }

        public static FileItem Open(string filename)
        {
            FileItem result = new FileItem();
            result.FileName = filename;
            result.Stream = File.Open(filename, FileMode.Open);
            return result;
        }

        public static FileItem Create(string filename)
        {
            FileItem result = new FileItem();
            result.FileName = filename;
            result.Stream = File.Open(filename, FileMode.CreateNew);
            return result;
        }

        public void Delete()
        {
            this.Close();
            File.Delete(this.FileName);
        }

        public void Close()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream.Dispose();
                Stream = null;
            }
        }
    }
}

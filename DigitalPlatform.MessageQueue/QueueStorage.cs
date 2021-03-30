using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Nito.AsyncEx;

namespace DigitalPlatform.Messaging
{
    /// <summary>
    /// 队列的磁盘存储机制
    /// </summary>
    public class QueueStorage : IDisposable
    {
        // 当前被引用的个数
        public int RefCount { get; set; }

        string _prefix = null;

        AsyncReaderWriterLock _lock = new AsyncReaderWriterLock();

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
            using var locker = _lock.WriterLock();

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
            prefix = prefix.ToLower();

            // 去掉扩展名部分
            string directory = Path.GetDirectoryName(prefix);
            string filename = Path.GetFileNameWithoutExtension(prefix);

            prefix = Path.Combine(directory, filename);

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

        static string GetNumber(string filename, string prefix)
        {
            filename = filename.ToLower();
#if DEBUG
            if (filename.Length <= prefix.Length)
                throw new Exception($"filename('{filename}') 应该比 prefix('{prefix}') 更长才对");
            if (filename.StartsWith(prefix) == false)
                throw new Exception($"filename('{filename}') 应该包含 prefix('{prefix}')");
#endif
            string number = filename.Substring(prefix.Length + 1);
            int end = number.IndexOf(".");
            if (end == -1)
                return null;
            return number.Substring(0, end);
        }

#if REMOVED
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
#endif

        // TODO: 优化为只打开第一个和最尾一个文件，中间的不打开
        static List<FileItem> InitializeDataFiles(string prefix)
        {
            // 搜集文件名中的编号
            string directory = Path.GetDirectoryName(prefix);
            string filename_prefix = Path.GetFileName(prefix);
            var di = new DirectoryInfo(directory);
            var fis = di.GetFiles($"{filename_prefix}_*{FileItem.Extention}");

            List<int> numbers = new List<int>();
            foreach (var fi in fis)
            {
                string number = GetNumber(fi.FullName, prefix);
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
                var item = FileItem.Open(prefix, value);
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
                /*
                string tailNumber = GetNumber(Path.GetFileName(tailFile.FileName), _prefix);
                if (Int32.TryParse(tailNumber, out int value) == false)
                    throw new Exception($"文件名 '{tailFile.FileName}' 格式错误");
                */
                number = tailFile.Number + 1;
            }

            var item = FileItem.Create(_prefix, number);
            _dataFiles.Add(item);
            return item;
        }

        // 找到最后一个数据文件
        FileItem PrepareTailFile(int data_length)
        {
            FileItem tailFile = null;
            if (_dataFiles.Count > 0)
            {
                tailFile = _dataFiles[_dataFiles.Count - 1];

                // 观察这个数据文件的大小。如果追加后会超过极限，则新增一个文件
                if (tailFile.Stream.Length > 0
                    && tailFile.Stream.Length + data_length + 5 > _chunkSize)
                    tailFile = NewDataFile();
            }
            else
                tailFile = NewDataFile();

            return tailFile;
        }

        // 追加一个数据单元
        public void Append(byte[] data)
        {
            using var locker = _lock.WriterLock();

            var tailFile = PrepareTailFile(data.Length);

            AppendData(tailFile.Stream, data).Wait();
        }

        // 追加一个数据单元
        public async Task AppendAsync(byte[] data)
        {
            using var locker = _lock.WriterLock();

            var tailFile = PrepareTailFile(data.Length);

            await AppendData(tailFile.Stream, data);
        }

        static async Task AppendData(Stream stream,
            byte[] data)
        {
            stream.Seek(0, SeekOrigin.End);

            // 写入长度 4 bytes
            byte[] buffer = BitConverter.GetBytes((Int32)(data.Length + 5));
            // Array.Clear(buffer, 0, buffer.Length);
            await stream.WriteAsync(buffer);
            // 写入状态 byte。1 表示有内容
            await stream.WriteAsync(new byte[] { 1 });
            // 写入数据
            await stream.WriteAsync(data);
        }

        // 获得并移走一个数据单元
        public async Task<byte[]> GetAsync(bool remove)
        {
            using var locker = _lock.WriterLock();

        REDO:
            if (_dataFiles.Count == 0)
                return null;    // 表示没有任何数据了
                                // 找到第一个数据文件
            FileItem firstFile = null;
            firstFile = _dataFiles[0];

            var get_result = await firstFile.GetFirstDataAsync();
            if (get_result == null)
            {
                // 文件是空的
                _dataFiles.RemoveAt(0);
                firstFile.Delete();
                goto REDO;
            }

            // 从中移走数据。如果文件空了，要删除这个文件
            if (remove)
                MaskBlankData(firstFile.Stream, get_result.Offset);

            // 如果必要，还需要收缩索引文件空闲空间

            return get_result.Data;
        }

        // 标记空白块
        static void MaskBlankData(Stream stream, long offset)
        {
#if DEBUG
            if (stream.Length <= offset)
                throw new Exception($"offset={offset} 越过文件末尾 {stream.Length}");
#endif
            FileItem.FastSeek(stream, offset + 4);
            stream.WriteByte(0);
        }

        [Flags]
        enum BlockType
        {
            Blank = 0x01,
            Data = 0x02,
        }

        static long GetCount(Stream stream, BlockType block_type)
        {
            long count = 0;
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
                    if ((block_type & BlockType.Blank) != 0)
                        count++;
                }
                else
                {
                    if ((block_type & BlockType.Data) != 0)
                        count++;
                }

                // 跳过当前块，到下一块
                stream.Seek(length - 5, SeekOrigin.Current);
            }

            return count;
        }

        public long GetFileCount()
        {
            using var locker = _lock.ReaderLock();

            return _dataFiles.Count;
        }

        public long GetDataCount()
        {
            using var locker = _lock.ReaderLock();

            long result = 0;
            foreach (var file in _dataFiles)
            {
                result += GetCount(file.Stream, BlockType.Data);
            }

            return result;
        }

        public long GetBlankCount()
        {
            using var locker = _lock.ReaderLock();

            long result = 0;
            foreach (var file in _dataFiles)
            {
                result += GetCount(file.Stream, BlockType.Blank);
            }

            return result;
        }

        // 取得第一个数据单元的内容，但并不移走它
        public async Task<byte[]> PeekAsync()
        {
            return await GetAsync(false);
        }

        public void Dispose()
        {
            this.Close();
        }

        // 清除所有数据文件
        public void Clear()
        {
            using var locker = _lock.WriterLock();

            if (_dataFiles != null)
            {
                foreach (var file in _dataFiles)
                {
                    file?.Delete();
                }
                _dataFiles.Clear();
            }
        }

        // 数据文件的结构是变长结构。每一块的头部有本块长度，和一个标志位。
        // 通过标志位能看出本块是否空闲
    }

    // 数据文件
    public class FileItem
    {
        public string FileName { get; set; }
        public Stream Stream { get; set; }
        public int Number { get; set; }

        // 第一个非空数据块的偏移。为了提高 Get() 的速度，避免从头开始遍历
        long _headOffset = -1;  // -1 表示尚未初始化
        public long HeadOffset
        {
            get
            {
                return _headOffset;
            }
            set
            {
                _headOffset = value;
            }
        }

        public const string Extention = ".data";

        public static string GetFilePath(string prefix, int value)
        {
            return $"{prefix}_{value.ToString()}{Extention}";
        }

        public static FileItem Open(string prefix, int number)
        {
            FileItem result = new FileItem();
            result.HeadOffset = -1;
            result.Number = number;
            result.FileName = GetFilePath(prefix, number);
            result.Stream = File.Open(result.FileName, FileMode.Open);
            return result;
        }

        public static FileItem Create(string prefix, int number)
        {
            FileItem result = new FileItem();
            result.HeadOffset = -1;
            result.Number = number;
            result.FileName = GetFilePath(prefix, number);
            result.Stream = File.Open(result.FileName, FileMode.CreateNew);
            return result;
        }

        public class GetFirstDataResult
        {
            public byte[] Data { get; set; }
            public long Offset { get; set; }
        }

        // 定位第一个数据单元
        public async Task<GetFirstDataResult> GetFirstDataAsync()
        {
            var stream = this.Stream;
            long offset = this.HeadOffset;

            if (offset == -1)
                stream.Seek(0, SeekOrigin.Begin);
            else
                FileItem.FastSeek(stream, offset);

            while (true)
            {
                // 读出长度 4 bytes
                byte[] buffer = new byte[4];
                var ret = await stream.ReadAsync(buffer);
                if (ret < 4)
                    break;
                Int32 length = BitConverter.ToInt32(buffer);

                // 读出状态 byte
                byte[] one = new byte[1];
                ret = await stream.ReadAsync(one);
                if (ret < 1)
                    break;
                // 空块
                if (one[0] == 0)
                {
                    // 跳过当前块，到下一块
                    stream.Seek(length - 5, SeekOrigin.Current);
                    continue;
                }

                // 读出数据
                offset = stream.Position - 5;

                byte[] data = new byte[length - 5];
                int read_length = await stream.ReadAsync(data);
                if (read_length < data.Length)
                    throw new Exception($"希望读出 {data.Length}，但只读出了 {read_length}");

                this.HeadOffset = offset;
                return new GetFirstDataResult
                {
                    Offset = offset,
                    Data = data
                };
            }

            this.HeadOffset = -1;
            return null;    // 全部都是空块
        }

        // 快速移动文件指针到相对于文件头部的 lOffset 位置
        // 根据要 seek 到的位置距离当前位置和文件头的远近，选择近的起点来进行移动
        public static void FastSeek(Stream stream, long lOffset)
        {
            long delta1 = lOffset - stream.Position;
#if NO
            if (delta1 < 0)
                delta1 = -delta1;
#endif
            if (Math.Abs(delta1) < lOffset)
            {
                stream.Seek(delta1, SeekOrigin.Current);
                Debug.Assert(stream.Position == lOffset, "");
            }
            else
                stream.Seek(lOffset, SeekOrigin.Begin);
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
                HeadOffset = -1;
            }
        }
    }
}

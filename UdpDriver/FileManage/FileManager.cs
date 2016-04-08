using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace UdpDriver
{
   public class FileManages:IDisposable
    {
        /// <summary>
        /// 文件流
        /// </summary>
        public  FileStream FileStream { get;private set; }
        /// <summary>
        /// 文件的总长度
        /// </summary>
        public long FileLength { get;private set; }
        /// <summary>
        /// 文件单元的总数
        /// </summary>
        public long FileUnitCount { get; set; }
        /// <summary>
        /// 单个文件单元的大小
        /// </summary>
        public int FileUnitSize { get; set; }
        /// <summary>
        /// 文件单元的索引
        /// </summary>
        public int FileIndex { get; set; }


        public FileManages()
        {
            FileUnitSize = 1024 * 5;
        }

        #region  发送文件流



        /// <summary>
        /// 建立文件流
        /// </summary>
        /// <param name="FilePath"></param>
        public void SendFileStreamSetup(string FilePath)
        {
            SendFileStreamCreate(FilePath);
        }
        /// <summary>
        /// 建立文件流
        /// </summary>
        /// <param name="path"></param>
        private void SendFileStreamCreate(string path)
        {
            FileStream = new FileStream(path,FileMode.Open,FileAccess.Read,FileShare.Read,FileUnitSize,true);
            FileLength = FileStream.Length;
            FileUnitCount = FileLength / FileUnitSize;
            if (FileLength % FileUnitSize != 0)
                FileUnitCount++;
        }
        /// <summary>
        /// 每次读取一个文件单元内容
        /// </summary>
        public void FileUnitRead()
        {
            int _index = FileIndex;
            int index = Interlocked.Increment(ref _index);
            FileIndex = _index;
            FileUnitRead(FileIndex);
        }
        private void FileUnitRead(int index)
        {
            int size = FileUnitSize;
            if ((FileLength - index * FileUnitSize) < FileUnitSize)
                size =(int)(FileLength - FileUnitSize);
            byte[] tmp = new byte[size];
            FileUnit obj = new FileUnit(index,tmp);
            FileStream.Position = index * FileUnitSize;
            FileStream.BeginRead(tmp,0,size,ar=> {
                int length = FileStream.EndRead(ar);
                FileUnit state = (FileUnit)ar.AsyncState;
                int _index = state.Index;
                byte[] buffer = state.BufferBytes;
                if(length<FileUnitSize)
                {
                    byte[] copeBytes = new byte[length];
                    Buffer.BlockCopy(buffer,0,copeBytes,0,length);
                    OnFileReadEvent(new FileReadArgs(_index,copeBytes));
                }
                else
                {
                    OnFileReadEvent(new FileReadArgs(_index,buffer));
                }

            },obj);

        }
        #region 文件读取事件
        /// <summary>
        /// 当文件单元读取好之后事件
        /// </summary>
        public event FileReadHandler FileReadEvent;
        protected void OnFileReadEvent(FileReadArgs e)
        {
            if (FileReadEvent != null)
                FileReadEvent(this,e);
        }
        #endregion

        #endregion


        #region 文件写入流

        public void ResFileStreamSetup(string path,long FileUnitCount)
        {
            this.FileUnitCount = FileUnitCount;
            ResFileStreamCreate(path);
        }
        private void ResFileStreamCreate(string path)
        {
            if (FileStream != null)
                Dispose();
            if (FileUnitSize == 0)
                FileUnitSize = 1024 * 5;
            FileStream = new FileStream(path, FileMode.OpenOrCreate, FileAccess.Write, FileShare.None, FileUnitSize, true);
            Stream.Synchronized(FileStream);

        }

        public void FileUnitWrite(int index,byte[] bytes)
        {
            FileStream.Position = index * FileUnitSize;
            byte[] tmp = new byte[bytes.Length];
            Buffer.BlockCopy(bytes,0,tmp,0,bytes.Length);
            FileStream.BeginWrite(tmp, 0, tmp.Length, ar => {
                FileStream.EndWrite(ar);
                int _index = (int)ar.AsyncState;
                if (_index ==FileUnitCount - 1)
                {
                    FileStream.Flush();
                    FileStream.Close();
                    FileStream.Dispose();
                    FileStream = null;
                }
            }, index);
        }



        #endregion










        public void Dispose()
        {
            if (FileStream != null)
            {
                FileStream.Close();
                FileStream.Dispose();
                FileStream = null;
            }
        }
    }
    public delegate void FileReadHandler(object sender, FileReadArgs e);
    public class FileReadArgs:EventArgs
    {
        public int Index { get; set; }
        public byte[] BufferBytes { get; set; }

        public FileReadArgs(int index,byte[] bytes)
        {
            Index = index;
            BufferBytes = new byte[bytes.Length];
            Buffer.BlockCopy(bytes,0,BufferBytes,0,bytes.Length);
        }
    }
}

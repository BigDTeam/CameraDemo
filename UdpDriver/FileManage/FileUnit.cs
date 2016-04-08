using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
    /// <summary>
    /// 存储临时的文件块信息，包括索引和文件字节
    /// </summary>
    [Serializable]
    class FileUnit
    {
        /// <summary>
        /// 文件块索引
        /// </summary>
        public int Index { get; set; }
        /// <summary>
        /// 文件块字节
        /// </summary>
        public byte[] BufferBytes { get; set; }

        public FileUnit(int index,byte[] bytes)
        {
            Index = index;
            BufferBytes = new byte[bytes.Length];
            Buffer.BlockCopy(bytes,0,BufferBytes,0,bytes.Length);
        }
    }
}

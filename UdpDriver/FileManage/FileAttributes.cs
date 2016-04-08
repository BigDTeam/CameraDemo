using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
    /// <summary>
    /// 文件的属性，包括文件名，大小，文件单元数量和大小
    /// </summary>
    [Serializable]
    class FileAttributes
    {
        public string FileName { get; set; }
        public long FileLength { get; set; }
        public long FileUnitCount { get; set; }
        public int FileUnitSize { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
    [Serializable]
    class SerializeFile
    {
        public FileHeadType MessageHead { get; set; }
        public object Obj { get; set; }

        public SerializeFile() { }
        public SerializeFile(FileHeadType type,object obj)
        {
            MessageHead = type;
            Obj = obj;
        }
        /// <summary>
        /// 将对象的序列化和消息头合并转为字节数据
        /// </summary>
        /// <returns></returns>
        public byte[] ToBytes()
        {
            byte[] data = Util.Serialize(Obj);
            byte[] header = Encoding.UTF8.GetBytes(MessageHead.ToString());
            byte[] buffer = new byte[data.Length + header.Length];
            Buffer.BlockCopy(header, 0, buffer, 0, header.Length);
            Buffer.BlockCopy(data, 0, buffer,header.Length, data.Length);
            return buffer;
        }
        /// <summary>
        /// 从buffer中取出messsageHead && Obj对象
        /// </summary>
        /// <param name="buffer"></param>
        public void FromBytes(byte[] buffer)
        {
            int size=sizeof(FileHeadType);
            string headStr = Encoding.UTF8.GetString(buffer,0,size);
            FileHeadType HeadType;
            Enum.TryParse(headStr,out HeadType);
            MessageHead = HeadType;
            Obj = Util.Deserialize(buffer, size);
        }
    }

    enum FileHeadType
    {
        FILEBEGIN=1<<0,
        FILECONTINUE=1<<1,
        FILEPAUSE=1<<2,
        FILESTOP=1<<3
    }
}

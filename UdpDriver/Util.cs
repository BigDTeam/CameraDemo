using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
    /// <summary>
    /// 消息头分类
    /// </summary>
   public enum MsgHeadType
    {
        Command=1<<0,
        Text=1<<1,
        Photo=1<<2,
        File=1<<3
    }


    public class Util
    {

        #region 拆箱信息
        /// <summary>
        ///获取信息体
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="validation"></param>
        /// <param name="msg"></param>
        public static void UnBoxMsg(byte[] buffer,MsgHeadType validation,ref string msg)
        {
            byte[] tmpBytes = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, tmpBytes, 0, buffer.Length);
            string tmpStr = Encoding.UTF8.GetString(tmpBytes);
            string[] msgStrs = tmpStr.Split('#');
            if (msgStrs.Length < 3)
                return;
            MsgHeadType valTmp;
            if (!Enum.TryParse<MsgHeadType>(msgStrs[0], out valTmp))
                return;
            if (!valTmp.Equals(validation))
                return;
            if (!msgStrs[2].Equals("@EOF"))
                return;
            msg = msgStrs[1];
        }
        /// <summary>
        ///获取信息体
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="validation"></param>
        /// <param name="msg"></param>
        public static void UnBoxMsg(byte[] buffer, MsgHeadType validation,Action<string> handler)
        {
            byte[] tmpBytes = new byte[buffer.Length];
            Buffer.BlockCopy(buffer, 0, tmpBytes, 0, buffer.Length);
            string tmpStr = Encoding.UTF8.GetString(tmpBytes);
            string[] msgStrs = tmpStr.Split('#');
            if (msgStrs.Length < 3)
                return;
            MsgHeadType valTmp;
            if (!Enum.TryParse<MsgHeadType>(msgStrs[0], out valTmp))
                return;
            if (!valTmp.Equals(validation))
                return;
            if (!msgStrs[2].Equals("@EOF"))
                return;
            handler(msgStrs[1]);
        }


        /// <summary>
        /// 获取信息体
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="validation"></param>
        /// <param name="msg"></param>
        public static void UnBoxMsg(byte[] buffer, MsgHeadType validation, ref byte[] msg)
        {
            string tmp = "";
            UnBoxMsg(buffer,validation,ref tmp);
            msg =Encoding.UTF8.GetBytes(tmp);
        }
        /// <summary>
        /// 获取信息体
        /// </summary>
        /// <param name="buffer"></param>
        /// <param name="validation"></param>
        /// <param name="msg"></param>
        public static void UnBoxMsg1(byte[] buffer, MsgHeadType validation,Action<byte[]> handler)
        {
            string tmp = "";
            UnBoxMsg(buffer, validation, ref tmp);
            handler(Encoding.UTF8.GetBytes(tmp));
        }

        #endregion


        #region 装箱信息

        /// <summary>
        /// 装箱信息，字节型
        /// </summary>
        public static void BoxMsg(byte[] buffer, MsgHeadType validation, out byte[] MsgBytes)
        {
            byte[] msgHead = Encoding.UTF8.GetBytes(validation.ToString()+"#");
            byte[] msgRoot= Encoding.UTF8.GetBytes("#@EOF");
            MsgBytes = new byte[msgHead.Length+buffer.Length+msgRoot.Length];
            Buffer.BlockCopy(msgHead,0,MsgBytes,0,msgHead.Length);
            Buffer.BlockCopy(buffer,0, MsgBytes, msgHead.Length, buffer.Length);
            Buffer.BlockCopy(msgRoot, 0, MsgBytes, msgHead.Length+buffer.Length, msgRoot.Length);
        }

        /// <summary>
        /// 装箱信息，字节型
        /// </summary>
        public static void BoxMsg(byte[] buffer, MsgHeadType validation,Action<byte[]> handler)
        {
            byte[] msgHead = Encoding.UTF8.GetBytes(validation.ToString() + "#");
            byte[] msgRoot = Encoding.UTF8.GetBytes("#@EOF");
            byte[] MsgBytes = new byte[msgHead.Length + buffer.Length + msgRoot.Length];
            Buffer.BlockCopy(msgHead, 0, MsgBytes, 0, msgHead.Length);
            Buffer.BlockCopy(buffer, 0, MsgBytes, msgHead.Length, buffer.Length);
            Buffer.BlockCopy(msgRoot, 0, MsgBytes, msgHead.Length + buffer.Length, msgRoot.Length);
            handler(MsgBytes);
        }


        /// <summary>
        /// 装箱信息体，字符串型
        /// </summary>
        /// <param name="RawMsg"></param>
        /// <param name="validation"></param>
        /// <param name="msg"></param>
        public static void BoxMsg(string RawMsg, MsgHeadType validation, out string msg)
        {
            byte[] msgBytes;
            msg = "";
            byte[] strBytes = Encoding.UTF8.GetBytes(RawMsg);
            BoxMsg(strBytes,validation,out msgBytes);
            msg = Encoding.UTF8.GetString(msgBytes);
        }

        public static void BoxMsg1(string RawMsg, MsgHeadType validation,Action<string> handler)
        {
            byte[] msgBytes;
            string msg = "";
            byte[] strBytes = Encoding.UTF8.GetBytes(RawMsg);
            BoxMsg(strBytes, validation, out msgBytes);
            msg = Encoding.UTF8.GetString(msgBytes);
            handler(msg);
        }


        #endregion

        /// <summary>
        /// 将字符串和枚举类型做比较
        /// </summary>
        /// <param name="str"></param>
        /// <param name="head"></param>
        /// <returns></returns>
        public static bool EnumCompare(string str,MsgHeadType head)
        {
            MsgHeadType tmp;
            if (!Enum.TryParse(str,out tmp))
                return false;
            if (!tmp.Equals(head))
                return false;
            return true;
        }

        /// <summary>
        /// 将一个字符串型 转换为十六进制 tmple：0xAA 0xBB 0xCC 0x01
        /// </summary>
        /// <param name="HexStr"></param>
        /// <returns></returns>

        public static byte[] HexFromStr(string HexStr)
        {
            string str = HexStr;
            string[] tmp = str.Split(' ');
            List<byte> cmdlist = new List<byte>();
            foreach (string st in tmp)
            {
                string tmpx = st;
                tmpx.Substring(tmpx.Length-2,2).Trim();
                cmdlist.Add(Convert.ToByte(tmpx,16));
            }    
            byte[] cmd0 = cmdlist.ToArray();
            return cmd0;
        }

        /// <summary>
        /// 序列号字节
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static byte[] Serialize(object obj)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream();
            bf.Serialize(stream, obj);
            byte[] datas = stream.ToArray();
            stream.Dispose();
            return datas;
        }
        /// <summary>
        /// 反序列化
        /// </summary>
        /// <param name="datas"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public static object Deserialize(byte[] datas, int index)
        {
            BinaryFormatter bf = new BinaryFormatter();
            MemoryStream stream = new MemoryStream(datas, index, datas.Length - index);
            object obj = bf.Deserialize(stream);
            stream.Dispose();
            return obj;
        }
    }



}

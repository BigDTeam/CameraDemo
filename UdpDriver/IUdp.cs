using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
    public abstract class IUdp
    {
        /// <summary>
        /// 监听端口
        /// </summary>
        public int ListenPort { get; set; }

        public UdpClient _UdpDriver;
        /// <summary>
        /// Udp Client实例
        /// </summary>
        public UdpClient UdpDriver{
            get {
                if (_UdpDriver == null)
                {
                    _UdpDriver = new UdpClient(ListenPort);//绑定本地端口
                    uint IOC_IN = 0x80000000;
                    uint IOC_VENDOR = 0x18000000;
                    uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
                    UdpDriver.Client.IOControl(
                        (int)SIO_UDP_CONNRESET,
                        new byte[] { Convert.ToByte(false) },
                        null);
                }
                return _UdpDriver;
            }
            set { _UdpDriver = value; }
        }
        /// <summary>
        /// 监听方法
        /// </summary>
        /// <param name="Port"></param>
        /// <returns></returns>
        public abstract bool Listen(int Port);
        /// <summary>
        /// 发送字节数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract bool SendData(byte[] data,string ip,int port);
        /// <summary>
        /// 发送字符串数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract bool SendData(string data, string ip, int port);
        /// <summary>
        /// 发送十六进制数据
        /// </summary>
        /// <param name="data"></param>
        /// <param name="ip"></param>
        /// <param name="port"></param>
        /// <returns></returns>
        public abstract bool SendHexData(string data, string ip, int port);
        public abstract void Close();


        #region 事件
        /// <summary>
        /// 数据接收事件
        /// </summary>
        public event EventHandler<UdpDataArgs> DataInEvent;
        protected void OnDataInEvent(IPEndPoint remoteEp, byte[] data, MsgHeadType validation)
        {
            if (DataInEvent != null)
                DataInEvent(this, new UdpDataArgs(remoteEp,data,validation));
        }
        #endregion
    }

    public class UdpDataArgs:EventArgs
    {
        public string ClientIP { get; set; }
        public string DataStr { get; set; }
        public byte[] DataBytes { get; set; }

        public UdpDataArgs(IPEndPoint remoteEp,byte[] data,MsgHeadType validation)
        {
            ClientIP = remoteEp.Address.ToString();
            Handler(data, validation);
        }

        private void Handler(byte[] rawData,MsgHeadType validation)
        {
            Util.UnBoxMsg1(rawData,validation,msg=> {
                DataBytes = new byte[msg.Length];
                msg.CopyTo(DataBytes,0);});
            Util.UnBoxMsg(rawData,validation,msg=> {
                DataStr = msg; });
        }
    }
}

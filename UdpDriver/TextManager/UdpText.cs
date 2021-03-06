﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
   public class UdpText:IUdp
    {
        public override bool Listen(int Port)
        {
            if (Port == 0)
                ListenPort = 10009;//default listen Port
            ListenPort = Port;
            try
            {
                UdpDriver.BeginReceive(new AsyncCallback(ReceiveCallBack), null);
                return true;
            }
            catch
            {
                return false;
            }
        }

        private void ReceiveCallBack(IAsyncResult ar)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
            byte[] resultBytes = UdpDriver.EndReceive(ar, ref remoteEP);
            OnDataInEvent(remoteEP, resultBytes);
            UdpDriver.BeginReceive(new AsyncCallback(ReceiveCallBack), null);
        }

        public override bool SendData(string data, string ip, int port)
        {
            byte[] tmp = Encoding.UTF8.GetBytes(data);
            return SendData(tmp, ip, port);
        }
        public override bool SendData(byte[] data, string ip, int port)
        {
            IPEndPoint remoteEP = new IPEndPoint(IPAddress.Parse(ip), port);
            bool IsOk = false;
            Util.BoxMsg(data, MsgHeadType.Text, msg => {
                UdpDriver.BeginSend(msg, msg.Length, remoteEP, ar => {
                    int i = UdpDriver.EndSend(ar);
                    if (i == msg.Length)
                        IsOk = true;

                }, null);
            });
            return IsOk;
        }

        public override bool SendHexData(string data, string ip, int port)
        {
            byte[] tmp = Util.HexFromStr(data);
            return SendData(tmp, ip, port);

        }
        public override void Close()
        {
            UdpDriver.Close();
            UdpDriver = null;
        }

        protected void OnDataInEvent(IPEndPoint remoteEp, byte[] data)
        {
            OnDataInEvent(remoteEp, data, MsgHeadType.Text);
        }
    }
}

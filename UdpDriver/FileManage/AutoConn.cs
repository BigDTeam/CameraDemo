using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UdpDriver
{
   public class AutoConn:UdpCommand
    {
        private System.Timers.Timer ElapseTicker;
        /// <summary>
        /// 项目的客户端信息（ID，IP）
        /// </summary>
        public Dictionary<string,string> ClientsDic{get;set;}
        /// <summary>
        /// 项目的总客户端数
        /// </summary>
        public int ClientTotal { get; set; }

        /// <summary>
        /// 定时多少秒开始结束
        /// </summary>
        /// <param name="interval"></param>
        public AutoConn()
        {
            ClientsDic = new Dictionary<string, string>();
            
        }
        private int counter=0;
        /// <summary>
        /// 作为总服务器端，监听传递的信息,客户端不需要知道服务器IP地址，只需要向端口10009广播即可，客户端端口为10010
        /// Once ListenConnect is over,we get the all the Client ip information
        /// </summary>
        public void ListenClients(int interval, Action<Dictionary<string, string>> handler)
        {
            if (ClientTotal == 0)
                ClientTotal = 1;//设置默认客户端数量
            ElapseTicker = new System.Timers.Timer(1000);
            ElapseTicker.Enabled = true;
            ElapseTicker.Elapsed += delegate {
                if (counter == interval)
                {
                    ElapseTicker.Enabled = false;
                    counter = 0;
                    base.Close();
                    handler(ClientsDic);                    
                    return;
                }
                counter++;
            };
            Listen(10009);
            base.DataInEvent += (s, e) => {
                string ip = e.ClientIP;
                string id = e.DataStr;
                if (id == null || id == "")
                    return;
                if (ClientsDic.Count == ClientTotal)
                {
                    base.Close();
                    ElapseTicker.Enabled = false;
                    ElapseTicker = null;
                    handler(ClientsDic);
                    return;
                }
                base.SendData("Ack",ip,10010);
                if (!ClientsDic.ContainsKey(id) && ip != "" && id != "")
                    ClientsDic.Add(id, ip);
            };
        }


        #region  客户端

        private System.Timers.Timer SenderTicker;
        /// <summary>
        /// 进行监听服务器的IP地址，监听完成
        /// </summary>
        /// <param name="handler"></param>
        public  void ListenSvr(string ID,Action<string> handler)
        {
            Listen(10010);
            SenderTicker = new System.Timers.Timer(1000);
            SenderTicker.Enabled = true;
            SenderTicker.Elapsed += delegate {
                base.SendData(ID, "192.168.1.255", 10009);
            };  
            base.DataInEvent += (s, e) => {
                string ip = e.ClientIP;
                string ack = e.DataStr;
                if (ack == "Ack")
                {
                    SenderTicker.Enabled = false;
                    SenderTicker = null;
                    base.Close();
                    handler(ip);
                    return;
                }         
            };
        }
        #endregion

    }
}

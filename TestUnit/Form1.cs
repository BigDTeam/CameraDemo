using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;
using UdpDriver;
namespace TestUnit
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        IUdp udp;

        private void Form1_Load(object sender, EventArgs e)
        {
            udp = new UdpCommand();
            
            
            udp.Listen(10009);
            udp.DataInEvent += Udp_DataInEvent;

            AutoConn conn = new AutoConn();
            conn.ListenSvr("BR1",x=> {

            });
            
        }

        private void Udp_DataInEvent(object sender, UdpDataArgs e)
        {
            Console.WriteLine(e.ClientIP+":"+e.DataStr);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string greetWords =textBox1.Text.Trim();
            byte[] buffer = Encoding.UTF8.GetBytes(greetWords);
            udp.SendData(greetWords,"127.0.0.1",10010);        
        }
    }
    /// <summary>
    /// 消息头分类
    /// </summary>

}

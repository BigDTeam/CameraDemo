using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using CameraDemo.Model.NetSvr;

namespace CameraDemo.View
{
    public partial class MainView : Form
    {
        public MainView()
        {
            InitializeComponent();
            INetSvr test = new IpadEP();
            test.ProjectName = "hello";
            Console.WriteLine(test.ProjectName);
        }
    }
}

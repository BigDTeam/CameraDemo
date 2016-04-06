using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CameraDemo.Model.NetSvr
{
    abstract class INetSvr
    {
       public  string IPAddress { get; set; }
       public  int DefaultPort { get; set; }
       public   string ProjectName { get; set; }
       public  string ProjectID { get; set; }
    }
}

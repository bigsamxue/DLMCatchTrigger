using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator {
    public interface IConnection {

        bool IsConnected { get; }//查询连接状态
        event EventHandler NoError;//没有错误发生
        event EventHandler Error;//有错误发生
        int ErrorCode { get; }//错误代码

        void Connect();//连接设备，需要先设置连接方式与地址
        string RemoteCTRL(string message);//与设备通信，输入命令字符串;如无回复则返回null
        byte[] RequestBlockData(string message);//blockdata
        void Finish();//断开与设备连接
    }
}

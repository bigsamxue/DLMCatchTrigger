using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Communicator {
    public interface IParaSetup {
        int Wire { get; }//设定连接方式
        string Address { get; }//设定地址
        int ID { get; }//设备编号
        int Eos { get; }//设定terminator，默认为2
        int Eot { get; }//设定EOI使用，默认为1
        int TimeOut { get; }//设置超时，默认为50
        bool Status { get; }//允许状态
    }
}

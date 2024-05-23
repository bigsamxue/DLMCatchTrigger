using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TmctlAPINet;

namespace Communicator {
    public class Connection : IConnection, IParaSetup {
        protected TMCTL _cTmctl;
        public bool Status { protected set; get; }
        public bool IsConnected { protected set; get; } = false;

        public List<string> DeviceSerial { get; set; }
        protected int[] SearchDeviceSupportedList { get; set; }
        public enum wire {
            GPIB = 1,
            RS232 = 2,
            USB = 3,
            ETHER = 4,
            USBTMC = 5,
            ETHERUDP = 6,
            USBTMC2 = 7,
            VXI11 = 8,
            USB2 = 9,
            VISAUSB = 10,
            SOKET = 11,
            USBTMC3 = 12,
            USB3 = 13,
            HISLIP = 14,
        }
        public Dictionary<string, int> WireList = new Dictionary<string, int>{
            { "GPIB",1 },
            {"RS232",2 },
            {"USB",3 },
            {"ETHER",4 },
            {"USBTMC",5 },
            {"ETHERUDP",6 },
            {"USBTMC2",7},
            {"VXI-11",8 },
            {"USB2",9 },
            {"VISAUSB",10 },
            {"SOCKET",11 },
            {"USBTMC3",12 },
            {"USB3",13 },
            {"HISLIP",14 }
        };
        public Dictionary<int, string> ErrorCodeList = new Dictionary<int, string> {
            {0,     "No Error" },
            {1,     "Time Out" },
            {2,     "Target Device Not Found" },
            {4,     "Connection With The Device Failed" },
            {8,     "Not Connected To The Device" },
            {16,    "Already Connected To The Device" },
            {32,    "The PC Is Not Compatible" },
            {64,    "Illegal Function Parameter" },
            {256,   "Send Error" },
            {512,   "Receive Error" },
            {1024,  "Received Data Is Not Block Data" },
            {4096,  "System Error" },
            {8192,  "Illegal Device ID" },
            {16384, "Unsupported Function" },
            {32768, "Not Enough Buffer" },
            {65536, "Library Missing" }
        };
        protected wire _wire;
        public int Wire {
            set { if (!Enum.IsDefined(typeof(wire), value)) throw new ArgumentException("chose a correct wire"); _wire = (wire)value; }
            get { return (int)_wire; }
        }
        protected string _address;
        public string Address { protected set => _address = value; get => AddrCheck(_address); }
        int _id;
        public int ID { protected set => _id = value; get => _id; }
        int _errorCode;
        public int ErrorCode { get => _errorCode; }
        int _timeOut = 50;
        public int TimeOut { protected set => _timeOut = value; get => _timeOut; }

        protected enum eos { CRLF, CR, LF, EOI }
        protected enum eot { NotUse, Use }
        eos _eos = (eos)2;
        eot _eot = (eot)1;
        public int Eos {
            protected set { if (!Enum.IsDefined(typeof(eos), value)) throw new ArgumentException("eos must be 0,1,2 or 3"); _eos = (eos)value; }
            get { return (int)_eos; }
        }
        public int Eot {
            protected set { if (!Enum.IsDefined(typeof(eot), value)) throw new ArgumentException("eos must be 0 or 1"); _eot = (eot)value; }
            get { return (int)_eot; }
        }

        public Connection(int wire, string address) {                    //构造函数
            _cTmctl = new TMCTL();
            Wire = wire;
            Address = address;
            SearchDeviceSupportedList = new int[] { 10, 7, 12, 8 };
        }
        protected string AddrCheck(string address) {
            if (Wire == 7 || Wire == 12) {
                System.Text.StringBuilder addr = new System.Text.StringBuilder(100);
                _cTmctl.EncodeSerialNumber(addr, addr.Capacity, address);
                return addr.ToString();
            }
            return address;
        }
        //--------------------------ERROR--------------------------------------------------------
        public event EventHandler NoError;
        public event EventHandler Error;
        protected void OnCheckError(string location, int status) {             //在“location”步骤查询错误状态，若错误则触发Error事件，反之触发NoError事件
            if (status != 0) { GetErrorCode(); if (Error != null) Error(this, null); }
            else { Status = true; }
            if (NoError != null) NoError(this, null);
        }
        protected void GetErrorCode() {                                                //获取错误代码，状态置零
            Status = false;
            _errorCode = _cTmctl.GetLastError(_id);
        }
        public string ReportError() {                                          //显示错误代码（当错误时）
            return ($"{ErrorCodeList[ErrorCode]}");
        }
        //--------------------------------------------------------------------------------------

        protected bool Receiveflag(string message) {                         //判断message是否需要接受设备回复
            return message.IndexOf("?") >= 0 ? true : false;
        }
        public void Connect() {                                              //初始化与连接设置
            int status = 0;


            status = _cTmctl.Initialize(Wire, Address, ref _id);
            OnCheckError("Initialzation", status);

            status = _cTmctl.SetTerm(ID, Eos, Eot);
            OnCheckError("Terminator Setup", status);

            status = _cTmctl.SetTimeout(ID, TimeOut);
            OnCheckError("Timeout setup", status);

            if (status == 0) IsConnected = true;
        }

        public List<string> SearchDevice(int wire = 0, int max = 64) {
            DEVICELIST[] deviceList = new DEVICELIST[max];
            List<string> devices = new List<string>();
            StringBuilder encode = new StringBuilder(100);
            int num = 0;
            foreach (int i in SearchDeviceSupportedList) {
                if (i == 8) {
                    _cTmctl.SearchDevices(i, deviceList, max, ref num, "");
                    if (num > 0) {
                        for (int j = 0; j < num; j++) {
                            devices.Add(deviceList[j].adr.ToString());
                        }
                    }
                }
                else {
                    _cTmctl.SearchDevices(i, deviceList, max, ref num, "");

                    if (num > 0) {
                        for (int j = 0; j < num; j++) {
                            _cTmctl.DecodeSerialNumber(encode, encode.Capacity, deviceList[j].adr.ToString());
                            devices.Add(encode.ToString());
                        }
                    }
                }

            }

            return devices;
        }

        virtual public string RemoteCTRL(string message) {                          //通信
            if (!IsConnected) return "Not Connected";
            int status = 0;                                                 //状态
            int endflag = 1;                                                //
            int rlen = 0;                                                   //回复字符串长度    
            bool receiveflag;                                               //是否回复判断标志
            System.Text.StringBuilder buff = new System.Text.StringBuilder(1000000);
            receiveflag = Receiveflag(message);                             //判断是否有"？"？ 有回复:无回复
            if (receiveflag == true) {                                      //有回复
                status = _cTmctl.Send(_id, message);                         //发送
                OnCheckError("Sending", status);
                status = _cTmctl.Receive(_id, buff, buff.Capacity, ref rlen);//接收
                OnCheckError("Receiving", status);
                if (status != 0) return "Error:" + _errorCode.ToString();
                return buff.ToString();                                     //返回字符串
            }
            else {                                                          //无回复
                rlen = message.Length;
                status = _cTmctl.SendOnly(_id, message, rlen, endflag);      //只发送
                OnCheckError("SendOnly", status);
                if (status != 0) return "Error:" + _errorCode.ToString();
                return message + " Sent";                                     //返回空字符串
            }
        }
        public byte[] RequestBlockData(string message) {                    //未经过测试
            int status = 0;
            string buff = string.Empty;
            int length = 0;
            byte[] data;
            int datasize = 0;
            int totalsize = 0;
            int endflag = 0;

            status = _cTmctl.Send(_id, message);                             //发送
            OnCheckError("Send", status);
            status = _cTmctl.ReceiveBlockHeader(_id, ref length);
            OnCheckError("ReceiveBlockHeader", status);
            data = new byte[length + 1];
            while (endflag == 0) {
                status = _cTmctl.ReceiveBlockData(_id, ref data[totalsize], length, ref datasize, ref endflag);
                OnCheckError("ReceiveBlockData", status); if (status != 0) break;
                totalsize += datasize;
            }
            return data;
        }
        public void Finish() {                                               //断开与设备连接
            int status = 0;
            status = _cTmctl.Finish(_id);
            OnCheckError("Finishing", status);
            if (status == 0) IsConnected = false;
        }
    }
}

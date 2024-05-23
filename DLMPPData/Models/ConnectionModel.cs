using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLMPPData.Models {
    public class ConnectionModel : BindableBase {
        private string _serialNum;

        public string SerialNum {
            get { return _serialNum; }
            set { SetProperty(ref _serialNum, value); }
        }

        private string _connectStatus;

        public string ConnectStatus {
            get { return _connectStatus; }
            set { SetProperty(ref _connectStatus, value); }
        }

        private string _comboSelect;

        public string ComboSelect {
            get { return _comboSelect; }
            set { SetProperty(ref _comboSelect, value); }
        }


    }
}

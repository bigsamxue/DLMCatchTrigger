using Communicator;
using DLMPPData.Models;
using Prism.Commands;
using Prism.Mvvm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace DLMPPData.ViewModels {
    internal class ConnectionViewModel : BindableBase {
        public ConnectionViewModel() {
            this.ConnectionModel = new ConnectionModel();
            this.ConnectionModel.ComboSelect = "USBTMC2";
            this.ConnectionModel.ConnectStatus = "Connect";
            ImageSource = "pack://application:,,,/Icon/Disconnect.png";
            ConnectWay = new List<string>() { "USBTMC2", "USBTMC3", "VXI11" };

            SearchClickCommand = new DelegateCommand(SearchDevice);
            ConnectClickCommand = new DelegateCommand(ConnectDevice);
            FileClickCommand = new DelegateCommand(ChooseFilePath);


        }


        private string _info;
        public string Info {
            get { return _info; }
            set { SetProperty(ref _info, value); }
        }


        private string _imageSource;
        public string ImageSource {
            get { return _imageSource; }
            set { SetProperty(ref _imageSource, value); }
        }

        private ConnectionModel _connectionModel;
        public ConnectionModel ConnectionModel {
            get { return _connectionModel; }
            set { SetProperty(ref _connectionModel, value); }
        }

        private List<string> _connectWay;
        public List<string> ConnectWay {
            get { return _connectWay; }
            set { SetProperty(ref _connectWay, value); }
        }


        public DelegateCommand SearchClickCommand { get; set; }
        public DelegateCommand ConnectClickCommand { get; set; }
        public DelegateCommand FileClickCommand { get; set; }


        public bool IsSaveFileButton = false;



        public void SearchDevice() {
            if (App.DLM == null) {
                if (ConnectionModel.ComboSelect == "USBTMC2") {
                    App.DLM = new Connection((int)Connection.wire.USBTMC2, ConnectionModel.SerialNum);
                }

                else {
                    App.DLM = new Connection((int)Connection.wire.VXI11, ConnectionModel.SerialNum);
                }
            }
            List<string> dev = App.DLM.SearchDevice();
            if (dev.Count > 0) {
                for (int i = 0; i < dev.Count; i++) {
                    ConnectionModel.SerialNum = dev[i];
                }

                if (ConnectionModel.ComboSelect == "USBTMC2") {
                    App.DLM = new Connection((int)Connection.wire.USBTMC2, ConnectionModel.SerialNum);//这里因为只有一台DLM，多台的话dev[i]要分给不同的new Connection

                }
                else if (ConnectionModel.ComboSelect == "USBTMC3") {
                    App.DLM = new Connection((int)Connection.wire.USBTMC3, ConnectionModel.SerialNum);
                }
                else {
                    App.DLM = new Connection((int)Connection.wire.VXI11, ConnectionModel.SerialNum);
                }
            }
            else {
                ConnectionModel.SerialNum = "";
                System.Windows.MessageBox.Show("No Connection.Please check and search again");
            }
        }

        private void ConnectDevice() {
            if (App.DLM == null) {
                if (ConnectionModel.ComboSelect == "USBTMC2") {
                    App.DLM = new Connection((int)Connection.wire.USBTMC2, ConnectionModel.SerialNum);
                }
                else if (ConnectionModel.ComboSelect == "USBTMC3") {
                    App.DLM = new Connection((int)Connection.wire.USBTMC3, ConnectionModel.SerialNum);
                }
                else {
                    App.DLM = new Connection((int)Connection.wire.VXI11, ConnectionModel.SerialNum);
                }
            }
            if (App.DLM.IsConnected != true) {

                App.DLM.Connect();
                if (App.DLM.IsConnected == true) {
                    ConnectionModel.ConnectStatus = "Disconnect";

                    string idn = App.DLM.RemoteCTRL("*IDN?");
                    Info = idn;
                    ImageSource = "pack://application:,,,/Icon/Connect.png";
                }
                else {
                    string errorcode = App.DLM.ReportError();
                    System.Windows.MessageBox.Show("No Connection.Error Info：" + errorcode, "警告", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
                }
            }
            else {
                ConnectionModel.ConnectStatus = "Connect";
                App.DLM.Finish();
                ImageSource = "pack://application:,,,/Icon/Disconnect.png";
                App.DLM = null;
            }
        }

        private void ChooseFilePath() {
            IsSaveFileButton = true;
            FolderBrowserDialog dialog = new FolderBrowserDialog();
            dialog.SelectedPath = @"C:";
            dialog.ShowDialog();
            App.RelativePath = dialog.SelectedPath;
            Info = App.RelativePath;
        }

    }
}

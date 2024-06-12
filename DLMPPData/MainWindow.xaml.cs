using Communicator;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;

namespace DLMPPData {
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e) {
            MessageBoxResult result = System.Windows.MessageBox.Show("请确认是否关闭窗口?", "确认", MessageBoxButton.YesNo);
            if (result == MessageBoxResult.No) {
                e.Cancel = true;
            }
            else {
                if (App.DLM != null) {
                    App.DLM.Finish();
                }
            }

        }
    }
}

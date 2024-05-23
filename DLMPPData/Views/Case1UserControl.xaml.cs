using DLMPPData.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static DLMPPData.MainWindow;

namespace DLMPPData.Views {
    /// <summary>
    /// Case1UserControl.xaml 的交互逻辑
    /// </summary>
    public partial class Case1UserControl : UserControl {
        public Case1UserControl() {
            InitializeComponent();
            List<string> chlist = new List<string> { "1", "2", "3", "4" };
            chbox1.ItemsSource = chlist;
            chbox2.ItemsSource = chlist;
            chbox3.ItemsSource = chlist;
            mathchbox1.ItemsSource = chlist;
            mathchbox2.ItemsSource = chlist;
        }

        public List<string> DateList { get; set; }
        public List<string> C1MaxList { get; set; }
        public List<string> C2MinList { get; set; }
        public List<string> C3MinList { get; set; }
        public List<string> M1IntegList { get; set; }
        public List<string> M2MinList { get; set; }

        public int Status_condition { get; set; }
        public string Status { get; set; }
        public string Status_trg { get; set; }
        public string Status_run { get; set; }

        public int RecordCount { get; set; }

        public ConvertToDoubleClass ConvertClass { get; set; }

        //多线程下对DatagridView执行Add操作，集合必须要求是 ObservableCollection<T> 类型的
        public ObservableCollection<Elements> PPData;

        public CancellationTokenSource cancellationTokenSource { get; set; }
        public Task TaskForGetValue { get; set; }


        private void Run_Stop_Click(object sender, RoutedEventArgs e) {

            if (App.DLM == null || App.DLM.IsConnected != true) {
                MessageBox.Show("请先连接仪器", "警告", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            else {

                if (cancellationTokenSource != null) {
                    cancellationTokenSource.Cancel();
                }
                while (true) {

                    string status_temp = App.DLM.RemoteCTRL(":STATus:CONDition?");
                    string status_temp_1 = Regex.Replace(status_temp, @"[\r\n]", "");
                    int status_condition_1 = -1;
                    if (int.TryParse(status_temp_1, out status_condition_1)) {
                        Status_condition = status_condition_1;
                        Status = Convert.ToString(Status_condition, 2).PadLeft(16, '0');
                        Status_run = Status.Substring(Status.Length - 1, 1);
                        break;
                    }
                    else {
                        Thread.Sleep(1000);
                    }
                }

                if (Status_run == "0") {
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox1.Text}:MAX:STATE ON");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox2.Text}:MIN:STATE ON");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox3.Text}:MIN:STATE ON");
                    App.DLM.RemoteCTRL($":MEASURE:MATH{mathchbox1.Text}:TY2Integ:STATE ON");
                    App.DLM.RemoteCTRL($":MEASURE:MATH{mathchbox2.Text}:MIN:STATE ON");

                    var date = DateTime.Now.ToString("yyyyMMdd");
                    var time = DateTime.Now.ToString("HH.mm.ss");
                    App.FileName = App.RelativePath + "/DataFile" + date + " " + time + ".csv";
                    App.DLM.RemoteCTRL(":STARt");
                    Button_RunStop.Background = Brushes.LightGreen;
                    cancellationTokenSource = new CancellationTokenSource();   //cancellationToken每次Cancel（StopClick中）需要重新new
                    TaskForGetValue = Task.Run(GetValue, cancellationTokenSource.Token);
                }
                else {
                    if (cancellationTokenSource != null) {
                        cancellationTokenSource.Cancel();

                        if (!Directory.Exists(App.RelativePath)) {
                            Directory.CreateDirectory(App.RelativePath);
                        }

                        if (!File.Exists(App.FileName))
                            File.Create(App.FileName).Close();

                        StreamWriter sw = new StreamWriter(App.FileName, true, Encoding.UTF8);
                        string header1 = $"Max(C{chbox1.Text})";
                        string header2 = $"Min(C{chbox2.Text})";
                        string header3 = $"Min(C{chbox3.Text})";
                        string header4 = $"IntegTY(M{mathchbox1.Text})";
                        string header5 = $"Min(M{mathchbox2.Text})";


                        string dataHeader = $"Date,{header1},{header2},{header3},{header4},{header5}";
                        sw.WriteLine(dataHeader);
                        for (int j = 0; j < C1MaxList.Count; j++) {
                            sw.WriteLine($"{DateList[j]},{C1MaxList[j]},{C2MinList[j]},{C3MinList[j]},{M1IntegList[j]},{M2MinList[j]}");
                        }
                        sw.Flush();
                        sw.Close();
                    }
                    App.DLM.RemoteCTRL(":STOP");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox1.Text}:ALL OFF");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox1.Text}:COPY");
                    RecordCount = 0;
                    Button_RunStop.Background = Brushes.LightGray;
                }

            }



        }

        private void GetValue() {
            PPData = new ObservableCollection<Elements>();
            C1MaxList = new List<string>();
            C2MinList = new List<string>();
            C3MinList = new List<string>();
            M1IntegList = new List<string>();
            M2MinList = new List<string>();
            DateList = new List<string>();

            ConvertClass = new ConvertToDoubleClass();

            while (true) {
                if (cancellationTokenSource.Token.IsCancellationRequested) {
                    break;
                }
                string status_temp_2 = App.DLM.RemoteCTRL(":STATus:CONDition?");
                string status_temp_3 = Regex.Replace(status_temp_2, @"[\r\n]", "");
                int status_condition_Thread = -1;
                string status_Thread = null;
                string status_run_Thread = null;
                string status_trg_Thread = null;

                //因为需要调用主线程GUI的chbox控件的Text，因此需要App.Current.Dispatcher.Invoke
                App.Current.Dispatcher.Invoke(new Action(() => {

                    string recordCount_Thread_temp = App.DLM.RemoteCTRL($":MEASURE:CHANNEL{chbox1.Text}:MAX:COUNT?");
                    string recordCount_Thread_temp_1 = null;

                    if (recordCount_Thread_temp.Contains($":MEAS:CHAN{chbox1.Text}:MAX:COUN")) {
                        recordCount_Thread_temp_1 = Regex.Replace(recordCount_Thread_temp.Split(' ')[1], @"[\r\n]", "");
                    }
                    int RecordCount_Thread = -1;
                    if (int.TryParse(status_temp_3, out status_condition_Thread) && int.TryParse(recordCount_Thread_temp_1, out RecordCount_Thread)) {
                        status_Thread = Convert.ToString(status_condition_Thread, 2).PadLeft(16, '0');
                        status_run_Thread = status_Thread.Substring(status_Thread.Length - 1, 1);
                        status_trg_Thread = status_Thread.Substring(status_Thread.Length - 3, 1);
                    }


                    if (status_run_Thread == "1" && status_trg_Thread == "1" && RecordCount_Thread != 0 && RecordCount_Thread - RecordCount > 2) {
                        //MessageBox.Show($"RecordCount_Thread:{RecordCount_Thread},RecordCount:{RecordCount}");
                        RecordCount = RecordCount_Thread;

                        HeadName headName = new HeadName() { Header1 = "Date", Header2 = $"Max(C{chbox1.Text})", Header3 = $"Min(C{chbox2.Text})", Header4 = $"Min(C{chbox3.Text})", Header5 = $"IntegTY(M{mathchbox1.Text})", Header6 = $"Min(M{mathchbox2.Text})" };
                        DataContext = headName;

                        var CH1_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:CHANNEL" + chbox1.Text + ":MAX:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var CH1 = ConvertClass.ConvertToDouble(CH1_temp);

                        var CH2_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:CHANNEL" + chbox2.Text + ":MIN:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var CH2 = ConvertClass.ConvertToDouble(CH2_temp);

                        var CH3_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:CHANNEL" + chbox3.Text + ":MIN:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var CH3 = ConvertClass.ConvertToDouble(CH3_temp);

                        var M1_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:MATH" + mathchbox1.Text + ":TY2Integ:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var M1 = ConvertClass.ConvertToDouble(M1_temp);

                        var M2_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:MATH" + mathchbox2.Text + ":MIN:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var M2 = ConvertClass.ConvertToDouble(M2_temp);

                        var date_date_temp = App.DLM.RemoteCTRL(":SYSTem:CLOCk:DATE?");
                        var date_date = Regex.Replace(date_date_temp.Split(' ')[1].Replace("\"", ""), @"[\r\n]", "");

                        var date_time_temp = App.DLM.RemoteCTRL(":SYSTem:CLOCk:TIME?");
                        var date_time = Regex.Replace(date_time_temp.Split(' ')[1].Replace("\"", "").Replace("\"", ""), @"[\r\n]", "");
                        var date = date_date + "  " + date_time;

                        PPData.Add(new Elements() { Date = date, C1Max = CH1, C2Min = CH2, C3Min = CH3, M1IntegTY = M1, M2Min = M2 });
                        dgv.ItemsSource = PPData;
                        if (dgv.Items.Count > 0) {
                            var border = VisualTreeHelper.GetChild(dgv, 0) as Decorator;
                            if (border != null) {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) {
                                    scroll.ScrollToEnd();//最后一行
                                }
                            }
                        }

                        C1MaxList.Add(CH1);
                        C2MinList.Add(CH2);
                        C3MinList.Add(CH3);
                        M1IntegList.Add(M1);
                        M2MinList.Add(M2);
                        DateList.Add(date);
                    }
                }));

            }

        }


    }
    public class Elements {
        public string Date { get; set; }
        public string C1Max { get; set; }
        public string C2Min { get; set; }
        public string C3Min { get; set; }
        public string M1IntegTY { get; set; }
        public string M2Min { get; set; }
    }
    public class HeadName {
        public string Header1 { get; set; }
        public string Header2 { get; set; }
        public string Header3 { get; set; }
        public string Header4 { get; set; }
        public string Header5 { get; set; }
        public string Header6 { get; set; }
    }
}

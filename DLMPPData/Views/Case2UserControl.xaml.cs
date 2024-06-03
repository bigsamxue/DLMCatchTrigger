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

namespace DLMPPData.Views {
    /// <summary>
    /// Case2UserControl.xaml 的交互逻辑
    /// </summary>
    public partial class Case2UserControl : UserControl {
        public Case2UserControl() {
            InitializeComponent();
            List<string> chlist = new List<string> { "1", "2", "3", "4" };
            case2chbox1.ItemsSource = chlist;
            case2chbox2.ItemsSource = chlist;
            FindChild = new FindChild();
            FindWindow = new FindWindow();
        }
        public List<string> DateList2 { get; set; }
        public List<string> C1MinList { get; set; }
        public List<string> C2MaxList { get; set; }

        public int Status_condition2 { get; set; }
        public string Status2 { get; set; }
        public string Status_trg2 { get; set; }
        public string Status_run2 { get; set; }

        public int RecordCount { get; set; }

        public ConvertToDoubleClass ConvertClass { get; set; }

        //多线程下对DatagridView执行Add操作，集合必须要求是 ObservableCollection<T> 类型的
        public ObservableCollection<Elements2> PPData2;

        public CancellationTokenSource cancellationTokenSource2 { get; set; }
        public Task TaskForGetValue2 { get; set; }


        public FindChild FindChild { get; set; }
        public FindWindow FindWindow { get; set; }

        private void Run_Stop_Click2(object sender, RoutedEventArgs e) {
            if (App.DLM == null || App.DLM.IsConnected != true) {
                MessageBox.Show("请先连接仪器", "警告", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK);
            }
            else {

                if (cancellationTokenSource2 != null) {
                    cancellationTokenSource2.Cancel();
                }
                while (true) {

                    string status_temp = App.DLM.RemoteCTRL(":STATus:CONDition?");
                    string status_temp_1 = Regex.Replace(status_temp, @"[\r\n]", "");
                    int status_condition_1 = -1;
                    if (int.TryParse(status_temp_1, out status_condition_1)) {
                        Status_condition2 = status_condition_1;
                        Status2 = Convert.ToString(Status_condition2, 2).PadLeft(16, '0');
                        Status_run2 = Status2.Substring(Status2.Length - 1, 1);
                        break;
                    }
                    else {
                        Thread.Sleep(1000);
                    }
                }

                if (Status_run2 == "0") {
                    Window main = FindWindow.GetParentWindow(this);
                    //TODO：获取Window中的TabControl，其名字为tabItem1，将其IsEnable属性取反
                    TabItem tab1 = FindChild.FindVisualChild<TabItem>(main, "tabItem1");
                    TabItem tab2 = FindChild.FindVisualChild<TabItem>(main, "tabItem2");
                    if (tab1 != null) {
                        tab1.IsEnabled = false;
                        tab2.IsEnabled = true;
                    }
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{case2chbox1.Text}:MIN:STATE ON");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{case2chbox2.Text}:MAX:STATE ON");

                    var date = DateTime.Now.ToString("yyyyMMdd");
                    var time = DateTime.Now.ToString("HH.mm.ss");
                    App.FileName = App.RelativePath + "/DataFile" + date + " " + time + ".csv";
                    App.DLM.RemoteCTRL(":STARt");
                    Button_RunStop2.Background = Brushes.LightGreen;
                    cancellationTokenSource2 = new CancellationTokenSource();   //cancellationToken每次Cancel（StopClick中）需要重新new
                    TaskForGetValue2 = Task.Run(GetValue2, cancellationTokenSource2.Token);
                }
                else {
                    Window main = FindWindow.GetParentWindow(this);
                    //TODO：获取Window中的TabControl，其名字为tabItem1，将其IsEnable属性取反
                    TabItem tab1 = FindChild.FindVisualChild<TabItem>(main, "tabItem1");
                    TabItem tab2 = FindChild.FindVisualChild<TabItem>(main, "tabItem2");
                    if (tab1 != null) {
                        tab1.IsEnabled = true;
                        tab2.IsEnabled = true;
                    }

                    if (cancellationTokenSource2 != null) {
                        cancellationTokenSource2.Cancel();

                        if (!Directory.Exists(App.RelativePath)) {
                            Directory.CreateDirectory(App.RelativePath);
                        }

                        if (!File.Exists(App.FileName))
                            File.Create(App.FileName).Close();

                        StreamWriter sw = new StreamWriter(App.FileName, true, Encoding.UTF8);
                        string header1 = $"Min(C{case2chbox1.Text})";
                        string header2 = $"Max(C{case2chbox2.Text})";


                        string dataHeader = $"Date,{header1},{header2}";
                        sw.WriteLine(dataHeader);
                        for (int j = 0; j < C1MinList.Count; j++) {
                            sw.WriteLine($"{DateList2[j]},{C1MinList[j]},{C2MaxList[j]}");
                        }
                        sw.Flush();
                        sw.Close();
                    }
                    App.DLM.RemoteCTRL(":STOP");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{case2chbox1.Text}:ALL OFF");
                    App.DLM.RemoteCTRL($":MEASURE:CHANNEL{case2chbox1.Text}:COPY");
                    RecordCount = 0;
                    Button_RunStop2.Background = Brushes.LightGray;
                }

            }



        }

        private void GetValue2() {
            PPData2 = new ObservableCollection<Elements2>();
            C1MinList = new List<string>();
            C2MaxList = new List<string>();
            DateList2 = new List<string>();

            ConvertClass = new ConvertToDoubleClass();

            while (true) {
                if (cancellationTokenSource2.Token.IsCancellationRequested) {
                    break;
                }
                string status_temp_2 = App.DLM.RemoteCTRL(":STATus:CONDition?");
                string status_temp_3 = Regex.Replace(status_temp_2, @"[\r\n]", "");
                int Status_condition_Thread = -1;
                string status_Thread = null;
                string Status_run_Thread = null;
                string Status_trg_Thread = null;
                //因为需要调用主线程GUI的chbox控件的Text，因此需要App.Current.Dispatcher.Invoke
                App.Current.Dispatcher.Invoke(new Action(() => {
                    string RecordCount_Thread_temp = App.DLM.RemoteCTRL($":MEASURE:CHANNEL{case2chbox1.Text}:MIN:COUNT?");
                    string RecordCount_Thread_temp_1 = null;

                    if (RecordCount_Thread_temp.Contains($":MEAS:CHAN{case2chbox1.Text}:MIN:COUN")) {
                        RecordCount_Thread_temp_1 = Regex.Replace(RecordCount_Thread_temp.Split(' ')[1], @"[\r\n]", "");
                    }
                    int RecordCount_Thread = -1;
                    if (int.TryParse(status_temp_3, out Status_condition_Thread) && int.TryParse(RecordCount_Thread_temp_1, out RecordCount_Thread)) {
                        status_Thread = Convert.ToString(Status_condition_Thread, 2).PadLeft(16, '0');
                        Status_run_Thread = status_Thread.Substring(status_Thread.Length - 1, 1);
                        Status_trg_Thread = status_Thread.Substring(status_Thread.Length - 3, 1);

                    }

                    if (Status_run_Thread == "1" && Status_trg_Thread == "1" && RecordCount_Thread != 0 && RecordCount_Thread - RecordCount > 2) {
                        //MessageBox.Show($"RecordCount_Thread:{RecordCount_Thread},RecordCount:{RecordCount}");
                        RecordCount = RecordCount_Thread;

                        HeadName2 headName = new HeadName2() { Case2Header1 = "Date", Case2Header2 = $"Min(C{case2chbox1.Text})", Case2Header3 = $"Max(C{case2chbox2.Text})" };
                        DataContext = headName;

                        var CH1_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:CHANNEL" + case2chbox1.Text + ":MIN:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var CH1 = ConvertClass.ConvertToDoubleDivide1k(CH1_temp);


                        var CH2_temp = Regex.Replace(App.DLM.RemoteCTRL(":MEASURE:CHANNEL" + case2chbox2.Text + ":MAX:VALUE?").Split(' ')[1], @"[\r\n]", "");
                        var CH2 = ConvertClass.ConvertToDouble(CH2_temp);

                        var date_date_temp = App.DLM.RemoteCTRL(":SYSTem:CLOCk:DATE?");
                        var date_date = Regex.Replace(date_date_temp.Split(' ')[1].Replace("\"", ""), @"[\r\n]", "");

                        var date_time_temp = App.DLM.RemoteCTRL(":SYSTem:CLOCk:TIME?");
                        var date_time = Regex.Replace(date_time_temp.Split(' ')[1].Replace("\"", "").Replace("\"", ""), @"[\r\n]", "");
                        var date = date_date + "  " + date_time;

                        PPData2.Add(new Elements2() { Date2 = date, C1Min = CH1, C2Max = CH2 });
                        dgv2.ItemsSource = PPData2;

                        if (dgv2.Items.Count > 0) {
                            var border = VisualTreeHelper.GetChild(dgv2, 0) as Decorator;
                            if (border != null) {
                                var scroll = border.Child as ScrollViewer;
                                if (scroll != null) {
                                    scroll.ScrollToEnd();//最后一行
                                }
                            }
                        }

                        C1MinList.Add(CH1);
                        C2MaxList.Add(CH2);
                        DateList2.Add(date);
                    }
                }));
            }
        }
    }

    public class Elements2 {
        public string Date2 { get; set; }
        public string C1Min { get; set; }
        public string C2Max { get; set; }
    }

    public class HeadName2 {
        public string Case2Header1 { get; set; }
        public string Case2Header2 { get; set; }
        public string Case2Header3 { get; set; }

    }

}


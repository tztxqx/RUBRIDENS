using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Win32;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Research.DynamicDataDisplay;
using Microsoft.Research.DynamicDataDisplay.DataSources;
using System.Globalization;
using System.Collections.ObjectModel;

namespace RUBRIDENSii
{
    /// <summary>
    /// Single.xaml 的交互逻辑
    /// </summary>
    /// 

    public partial class Single : UserControl
    {

        #region 全局变量定义
        string Num = "";
        double Value = 0;
        ObservableCollection<Member3> memberData3 = new ObservableCollection<Member3>();
        Member3 dataGrid_SelectedElement = null;
        int MaxLine = 0;

        double sum = 0;//装入采集好的数据

        List<Ellipse> HaveMeasure = new List<Ellipse>();

        int bigORsmall = 0; //大小量程判定 0大量程1小量程
        #endregion

        public Single()
        {
            InitializeComponent();
            dataSource.Add(new ObservableDataSource<Point>());


            graph.Add(new LineGraph());


            if (PatientInfo.IsAdd == true)
                PatientInfo.IsAddAndSavefailed = true;

            foreach (UIElement uiEle in LayoutRoot.Children)
            {
                //WPF设计上的问题,Button.Clicked事件Supress掉了Mouse.MouseLeftButtonDown附加事件等.
                //不加这个Button、TextBox等无法拖动
                if (uiEle is Button || uiEle is Ellipse)
                {
                    uiEle.AddHandler(Button.MouseLeftButtonDownEvent, new MouseButtonEventHandler(Element_MouseLeftButtonDown), true);
                    uiEle.AddHandler(Button.MouseMoveEvent, new MouseEventHandler(Element_MouseMove), true);
                    uiEle.AddHandler(Button.MouseLeftButtonUpEvent, new MouseButtonEventHandler(Element_MouseLeftButtonUp), true);
                    continue;
                }
                //
                uiEle.MouseMove += new MouseEventHandler(Element_MouseMove);
                uiEle.MouseLeftButtonDown += new MouseButtonEventHandler(Element_MouseLeftButtonDown);
                uiEle.MouseLeftButtonUp += new MouseButtonEventHandler(Element_MouseLeftButtonUp);
            }         

            #region 颜色选择
            colorSelect[0] = Colors.Blue;
            colorSelect[1] = Colors.Green;
            colorSelect[2] = Colors.Red;
            colorSelect[3] = Colors.Pink;
            colorSelect[4] = Colors.Purple;
            colorSelect[5] = Colors.Yellow;
            colorSelect[6] = Colors.Black;
            colorSelect[7] = Colors.Navy;
            colorSelect[8] = Colors.Olive;
            #endregion

            #region epoint显示
            ellipse.Add(epoint2);
            #endregion


            if (PatientInfo.IsAdd == true)
                PatientInfo.IsAddAndSavefailed = true;
        }
        #region 属性
        SerialPort ComPort = new SerialPort();//声明一个串口      
        private string[] ports;//可用串口数组

        private bool ComPortIsOpen = false;//COM口开启状态字，在打开/关闭串口中使用，这里没有使用自带的ComPort.IsOpen，因为在串口突然丢失的时候，ComPort.IsOpen会自动false，逻辑混乱
        IList<customer> comList = new List<customer>();//可用串口集合
        private int ReadyOrStart = 0;//用来判断是ready还是Start 0表示从未开启，1表示已经准备过了，2表示Start状态
        private string shujvbao = " ";  //数据传输string
        private int currentSecond = 0; //当前时间的值
        private int timelimit;  //时间长度界面上可以更改5s,10s,15s;

        private Color[] colorSelect = new Color[9]; //线条9种颜色

        public byte[] setiao = { 0, 0, 143, 0, 0, 207, 0, 16, 255, 0, 80, 255, 0, 143, 255, 0, 207, 255, 16, 255, 239, 80, 255, 175, 143, 255, 111, 207, 255, 48, 255, 239, 0, 255, 175, 0, 255, 111, 0, 255, 48, 0, 239, 0, 0, 191, 0, 0 };//热力颜色

        //dataSource
        private List<ObservableDataSource<Point>> dataSource = new List<ObservableDataSource<Point>>();
        private List<LineGraph> graph = new List<LineGraph>();

        private List<Ellipse> ellipse = new List<Ellipse>();

        #endregion
        private void Window_Loaded(object sender, RoutedEventArgs e)//主窗口初始化
        {
            #region 可用串口下拉控件↓↓↓↓↓↓↓↓↓
            ports = SerialPort.GetPortNames();//获取可用串口
            if (ports.Length > 0)//ports.Length > 0说明有串口可用
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    comList.Add(new customer() { com = ports[i] });//下拉控件里添加可用串口
                }
                AvailableComCbobox.ItemsSource = comList;//资源路径
                AvailableComCbobox.DisplayMemberPath = "com";//显示路径
                AvailableComCbobox.SelectedValuePath = "com";//值路径
                AvailableComCbobox.SelectedValue = ports[0];//默认选第1个串口
            }
            else//未检测到串口
            {
                CMessageBox.Show("未连接到设备！", CMessageBoxButton.OK);
            }
            #endregion 可用串口下拉控件↑↑↑↑↑↑↑↑↑
            #region 默认设置↓↓↓↓↓↓↓↓↓
            ComPort.ReadTimeout = 8000;//串口读超时8秒
            ComPort.WriteTimeout = 8000;//串口写超时8秒，在1ms自动发送数据时拔掉串口，写超时5秒后，会自动停止发送，如果无超时设定，这时程序假死
            ComPort.ReadBufferSize = 1024;//数据读缓存
            ComPort.WriteBufferSize = 1024;//数据写缓存
            #endregion

            #region 画图属性
            graph[0] = plotter.AddLineGraph(dataSource[0], colorSelect[0], 2, Convert.ToString(0));

            plotter.LegendVisible = false;
            plotter.Viewport.FitToView();
            #endregion
            ComPort.DataReceived += new SerialDataReceivedEventHandler(ComReceive);//串口接收中断
        }
        private void timerTick(string buffer)
        {
            double x = currentSecond;
            double y;
            List<Point> point = new List<Point>();


            //标定更改
            y = biaoding(buffer.Substring(bigORsmall * 4, 4), bigORsmall);
            point.Add(new Point(x, y));
            dataSource[0].AppendAsync(base.Dispatcher, point[0]);

            currentSecond++;
        }
        private void Window_Closed(object sender, EventArgs e)//关闭窗口确认后closed ALT+F4
        {
            Application.Current.Shutdown();//先停止线程,然后终止进程.
            Environment.Exit(0);//直接终止进程.
        }
        public class customer//各下拉控件访问接口
        {
            public string com { get; set; }//可用串口
            public string com1 { get; set; }//可用串口                 
        }
        private void Button_Open(object sender, RoutedEventArgs e)//打开/关闭串口事件
        {
            if (AvailableComCbobox.SelectedValue == null)//先判断是否有可用串口
            {
                CMessageBox.Show("设备未连接！", CMessageBoxButton.OK);
                return;//没有串口，提示后直接返回
            }
            #region 打开串口
            if (ReadyOrStart == 1)
            {
                Console.Write("1");
                SetAfterClose();//成功关闭串口或串口丢失后的设置
                ComPort.DiscardInBuffer();//清接收缓存               
                ready_Button.Content = "准备";
                Reset();

                ComPort.Close();//关闭串口
            }
            if (ComPortIsOpen == false)//ComPortIsOpen == false当前串口为关闭状态，按钮事件为打开串口
            {
                timelimit = Convert.ToInt16(timeSelect.Text) - 1;
                PatientInfo.timelimit = timelimit;
                try//尝试打开串口
                {
                    ComPort.PortName = AvailableComCbobox.SelectedValue.ToString();//设置要打开的串口
                    ComPort.BaudRate = 115200;//设置当前波特率
                    ComPort.Parity = 0;//设置当前校验位
                    ComPort.DataBits = 8;//设置当前数据位
                    ComPort.StopBits = (StopBits)1;//设置当前停止位                    
                    ComPort.Open();//打开串口
                    PatientInfo.readyornot = false;
                    ReadyOrStart = 2; //设置指示
                    Reset();//重置
                }
                catch//如果串口被其他占用，则无法打开
                {
                    CMessageBox.Show("无法打开串口,请检测此串口是否有效或被其他占用！");
                    GetPort();//刷新当前可用串口
                    return;//无法打开串口，提示后直接返回
                }
                #endregion
                #region //↓↓↓↓↓↓↓↓↓成功打开串口后的设置↓↓↓↓↓↓↓↓↓
                openBtn.Content = "停止测量";//按钮显示改为“关闭按钮”
                ComPortIsOpen = true;//串口打开状态字改为true                
                AvailableComCbobox.IsEnabled = false;//失能可用串口控件

                BigRange.IsEnabled = false;
                SmallRange.IsEnabled = false;
                #endregion
            }
            #region 关闭串口           
            else
            {
                try//尝试关闭串口
                {
                    ComPort.DiscardOutBuffer();//清发送缓存
                    ComPort.DiscardInBuffer();//清接收缓存
                    ComPort.Close();//关闭串口
                    PatientInfo.readyornot = true;
                    ReadyOrStart = 0; //设置指示
                    openBtn.Content = "开始测量";
                    SetAfterClose();//成功关闭串口或串口丢失后的设置
                    Reset();
                }
                catch//如果在未关闭串口前，串口就已丢失，这时关闭串口会出现异常
                {
                    if (ComPort.IsOpen == false)//判断当前串口状态，如果ComPort.IsOpen==false，说明串口已丢失
                    {
                        SetComLose();
                    }
                    else//未知原因，无法关闭串口
                    {
                        CMessageBox.Show("无法关闭设备，原因未知！");
                        return;//无法关闭串口，提示后直接返回
                    }
                }
                BigRange.IsEnabled = true;
                SmallRange.IsEnabled = true;
            }
            #endregion

        }

        //移除数据
        private void openBtn_Copy1_Click(object sender, RoutedEventArgs e)
        {
            dataGrid_SelectedElement = (Member3)dataGrid1.SelectedItem;
            if (dataGrid_SelectedElement != null)
            {
                int delposition = memberData3.IndexOf(dataGrid_SelectedElement);
                LayoutRoot.Children.Remove(HaveMeasure[delposition]);
                HaveMeasure.Remove(HaveMeasure[delposition]);

                memberData3.Remove(dataGrid_SelectedElement);
                dataGrid1.DataContext = memberData3;
                MaxLine--;
            }
            else
            {
                //Do nothing
            }
        }

        private void ready_Button_Click(object sender, RoutedEventArgs e)//准备按钮
        {
            if (AvailableComCbobox.SelectedValue == null)//先判断是否有可用串口
            {
                CMessageBox.Show("未连接设备，无法测量");
                return;//没有串口，提示后直接返回
            }
            #region 打开串口
            if (ComPortIsOpen == false)//ComPortIsOpen == false当前串口为关闭状态，按钮事件为打开串口
            {
                try//尝试打开串口
                {
                    ComPort.PortName = AvailableComCbobox.SelectedValue.ToString();//设置要打开的串口
                    ComPort.BaudRate = 115200;//设置当前波特率
                    ComPort.Parity = 0;//设置当前校验位
                    ComPort.DataBits = 8;//设置当前数据位
                    ComPort.StopBits = (StopBits)1;//设置当前停止位                    
                    ComPort.Open();//打开串口
                    ReadyOrStart = 1;
                    PatientInfo.readyornot = false;
                    Reset();
                }

                catch//如果串口被其他占用，则无法打开
                {
                    CMessageBox.Show("设备被占用,请检测设备是否有效");
                    GetPort();//刷新当前可用串口
                    return;//无法打开串口，提示后直接返回
                }
                #endregion
                #region //↓↓↓↓↓↓↓↓↓成功打开串口后的设置↓↓↓↓↓↓↓↓↓

                ready_Button.Content = "取消准备";//按钮显示改为“取消准备”               
                ComPortIsOpen = true;//串口打开状态字改为true               
                AvailableComCbobox.IsEnabled = false;//失能可用串口控件
                BigRange.IsEnabled = false;
                SmallRange.IsEnabled = false;
                #endregion
            }
            #region 关闭串口
            else//ComPortIsOpen == true,当前串口为打开状态，按钮事件为关闭串口
            {

                try//尝试关闭串口
                {
                    SetAfterClose();//成功关闭串口或串口丢失后的设置
                    ready_Button.Content = "准备";
                    ComPort.DiscardInBuffer();//清接收缓存
                    Reset();

                    ComPort.Close();//关闭串口
                    PatientInfo.readyornot = true;
                    ReadyOrStart = 0;

                }
                catch//如果在未关闭串口前，串口就已丢失，这时关闭串口会出现异常
                {
                    if (ComPort.IsOpen == false)//判断当前串口状态，如果ComPort.IsOpen==false，说明串口已丢失
                    {
                        SetComLose();
                    }
                    else//未知原因，无法关闭串口
                    {
                        CMessageBox.Show("无法关闭设备，原因未知！");
                        return;//无法关闭串口，提示后直接返回
                    }
                }
                BigRange.IsEnabled = true;
                SmallRange.IsEnabled = true;
            }
            #endregion
        }

        private void Reset()  //取消准备后重置
        {
            currentSecond = 0;

           plotter.Children.Remove(graph[0]);

 
            dataSource.Clear();
            graph.Clear();
            dataSource.Add(new ObservableDataSource<Point>());
            graph.Add(new LineGraph());
           
            //更改量程
                dataSource.Add(new ObservableDataSource<Point>());
                graph.Add(new LineGraph());
                
            graph[0] = plotter.AddLineGraph(dataSource[0], colorSelect[0], 2, Convert.ToString(0));

       
            plotter.LegendVisible = false;
            plotter.Viewport.FitToView();
            recCount.Text = "0"; //接收计数
        }
        private void ComReceive(object sender, SerialDataReceivedEventArgs e)//接收数据 中断只标志有数据需要读取，读取操作在中断外进行
        {
            Thread.Sleep(20);//发送和接收均为文本时，接收中为加入判断是否为文字的算法，发送你（C4E3），接收可能识别为C4,E3，可用在这里加延时解决
            if (ComPortIsOpen)//如果已经开启接收
            {
                byte[] recBufferd;//接收缓冲区
                try
                {
                    if (ComPort.BytesToRead >= 12)//缓存数据包中的内容
                    {
                        recBufferd = new byte[ComPort.BytesToRead];//接收数据缓存大小
                        ComPort.Read(recBufferd, 0, recBufferd.Length);//读取数据 
                        int start = 0;
                        for (; start < recBufferd.Length; start++)
                        {
                            if ((int)recBufferd[start] == 83)  //判断是否为S
                                break;
                        }
                        byte[] recBuffer = new byte[12];
                        for (int i = 0; i < 12; i++)
                            recBuffer[i] = recBufferd[start++];
                        #region 判断是否合格的数据，并进行显示
                        try
                        {
                            string recData;//接收数据转码后缓存
                            recData = System.Text.Encoding.Default.GetString(recBuffer);//转码
                            #region 数据包判断并显示
                            if (recData[0] == 'S' && recData.Length == 12 && recData[11] == 'E') //根据协议处理，数据包头为00
                            {
                                shujvbao = "";
                                shujvbao = recData.Substring(3, 8);


                                // Console.WriteLine(shujvbao);
                                #region 文字\数据显示

                                UIAction(() =>
                                {
                                    checkBox1.Content = "当前压力值："  + biaoding(shujvbao.Substring(bigORsmall* 4, 4), bigORsmall
                                        ) + "mmHg";
                                   
                                    recCount.Text = (Convert.ToInt32(recCount.Text) + recBufferd.Length).ToString();//接收数据字节数
                                });
                                #endregion
                                #region 显示部分
                                Task tasknew = Task.Run
                                    //UIAction
                                    (() => timerTick(shujvbao));
                                tasknew.Wait();

                                #region 控件更改
                                UIAction(() =>
                                {

                                    int temp = string2int(shujvbao.Substring(bigORsmall * 4, 4), bigORsmall);
                                    ellipse[0].Fill = new SolidColorBrush(Color.FromRgb(setiao[temp * 3], setiao[temp * 3 + 1], setiao[temp * 3 + 2]));
                                    ellipse[0].ToolTip = biaoding(shujvbao.Substring(bigORsmall * 4, 4), bigORsmall);

                                });

                                
                                #region ReadyOrStart==2 显示测量
                                if (ReadyOrStart == 2) //如果为测量模式
                                {
                                    UIAction(() =>
                                    {

                                        timeSlider.Maximum = timelimit; //进度条
                                        timeSlider.Value = currentSecond;//进度条
                                        sum += biaoding(shujvbao.Substring(bigORsmall * 4, 4), bigORsmall);
                                        if ((currentSecond > timelimit) && (ReadyOrStart == 2)) //ComPortIsOpen == true,当前串口为打开状态，按钮事件为关闭串口
                                        {

                                            try//尝试关闭串口
                                            {
                                                ComPort.DiscardInBuffer();//清接收缓存
                                                SetAfterClose();//成功关闭串口或串口丢失后的设置
                                                ComPort.Close();//关闭串口
                                                ReadyOrStart = 0; //设置指示
                                                CMessageBoxResult confirmToDel = CMessageBox.Show("测量完成", CMessageBoxButton.OK);
                                                
                                                
                                                //测量完成加入数据
                                                string NumTemp = epoint2.Margin.ToString();
                                                int NumLen=0;
                                                for(int commNum=0;commNum<2;NumLen++)
                                                {
                                                    if(NumTemp[NumLen]==',')
                                                        commNum += 1;
                                                }
                                                Num = NumTemp.Substring(0, NumLen-1);
                                                sum = sum / (timelimit+1);   //平均值

                                                int _b = (int)(sum* 100);

                                                Value= (double)_b / 100;

                                                sum = 0;

                                                #region 标记ellipse
                                                //标记新的ellipse
                                                Ellipse justMeasure = new Ellipse();
                                                HaveMeasure.Add(justMeasure);
                                                justMeasure.VerticalAlignment = VerticalAlignment.Top;
                                                justMeasure.HorizontalAlignment = HorizontalAlignment.Left;
                                                justMeasure.Width = 10;
                                                justMeasure.Height = 10;
                                                justMeasure.Margin = epoint2.Margin;
                                                LayoutRoot.Children.Add(justMeasure);
                                                justMeasure.Fill = new SolidColorBrush(Colors.Pink);
                                                justMeasure.ToolTip = Value.ToString();
                                                #endregion

                                                #region 在grid中添加数据
                                                //添加新的数据项
                                                
                                                    memberData3.Add(new Member3()
                                                       {//Name病历号 Age姓名
                                                           Number = Num.ToString(),
                                                           Value = Value.ToString(),
                                                           EMargin = epoint2.Margin.ToString(),
                                                       });
                                                dataGrid1.DataContext = memberData3;
                                                #endregion

                                                BigRange.IsEnabled = true;
                                                SmallRange.IsEnabled = true;
                                                PatientInfo.finishtime = System.DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                                                openBtn.Content = "再次测量";
                                            }
                                            catch//如果在未关闭串口前，串口就已丢失，这时关闭串口会出现异常
                                            {
                                                if (ComPort.IsOpen == false)//判断当前串口状态，如果ComPort.IsOpen==false，说明串口已丢失
                                                {
                                                    SetComLose();
                                                }
                                                else//未知原因，无法关闭串口
                                                {
                                                    CMessageBox.Show("无法关闭设备，原因未知！");
                                                    return;//无法关闭串口，提示后直接返回
                                                }
                                            }
                                        }
                                    });
                                }
                                #endregion
                                if (ReadyOrStart == 2) //测量模式保存数据
                                {
                                    PatientInfo.PatientData += shujvbao; //记录当前保存的数据 
                                }
                                //Thread.Sleep(10);
                                #endregion
                                #endregion
                            }
                            else
                            {
                                if (recData[0] != 0)
                                {
                                    Console.WriteLine("Drop data because of incorrect header");
                                }
                                else
                                {
                                    Console.WriteLine("Drop data because of incrrocet length");
                                }
                            }
                            #endregion
                        }
                        catch (Exception ed)
                        {
                            Console.WriteLine("error" + ed.Message);
                        }
                        #endregion
                    }
                }
                catch (Exception ed)
                {
                    CMessageBox.Show(ed.Message);
                    UIAction(() =>
                    {
                        if (ComPort.IsOpen == false)//如果ComPort.IsOpen == false，说明串口已丢失
                        {
                            SetComLose();//串口丢失后相关设置
                        }
                        else
                        {
                            CMessageBox.Show("无法接收数据，原因未知！");
                        }
                    });
                }
            }
        }
        public double biaoding(string data, int i) //标定函数
        {

            double a = Convert.ToDouble(data);
            double b=a;
            if (i == 1)
                b = a * 0.1272;
            else if (i == 0)
                b = a * 2.363;
            int _b = (int)( b * 100);

            b = (double)_b / 100;           //保留两位有效数字
            return b;
        }
        public int string2int(string s, int i) //转为合适的颜色
        {
            double a = biaoding(s, i) / 200 * setiao.Length;
            if (a > setiao.Length / 3 - 1)
                a = setiao.Length / 3 - 1;
            return Convert.ToInt16(a);
        }
        void UIAction(Action action)//在主线程外激活线程方法
        {
            System.Threading.SynchronizationContext.SetSynchronizationContext(new System.Windows.Threading.DispatcherSynchronizationContext(App.Current.Dispatcher));
            Thread.Yield();
            System.Threading.SynchronizationContext.Current.Post(_ => action(), null);
        }
        private void SetAfterClose()//成功关闭串口或串口丢失后的设置
        {
            ComPortIsOpen = false;//串口状态设置为关闭状态
        }
        private void SetComLose()//成功关闭串口或串口丢失后的设置
        {
            CMessageBox.Show("设备丢失");
            GetPort();//刷新可用串口
            SetAfterClose();//成功关闭串口或串口丢失后的设置
        }
        private void AvailableComCbobox_PreviewMouseDown(object sender, MouseButtonEventArgs e)//刷新可用串口
        {
            GetPort();//刷新可用串口
        }
        private void GetPort()//刷新可用串口
        {
            comList.Clear();//情况控件链接资源
            ports = new string[SerialPort.GetPortNames().Length];//重新定义可用串口数组长度
            ports = SerialPort.GetPortNames();//获取可用串口
            if (ports.Length > 0)//有可用串口
            {
                for (int i = 0; i < ports.Length; i++)
                {
                    comList.Add(new customer() { com = ports[i] });//下拉控件里添加可用串口
                }
                AvailableComCbobox.ItemsSource = comList;//可用串口下拉控件资源路径
                AvailableComCbobox.DisplayMemberPath = "com";//可用串口下拉控件显示路径
                AvailableComCbobox.SelectedValuePath = "com";//可用串口下拉控件值路径
            }
        }
        private void saveBtn_Click(object sender, RoutedEventArgs e)//保存数据
        {
            if (dataGrid1.Items.Count == 0)
            {
                CMessageBox.Show("无信息保存！", CMessageBoxButton.OK);
            }
            else
            {
                try
                {
                    Console.WriteLine(PatientInfo.PatientData);
                    if (PatientInfo.IsAdd == true)
                    {
                        FileStream fs = new FileStream("singleshow/"+PatientInfo.NameToSave + ".fot", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                        StreamWriter sw = new StreamWriter(fs); // 创建写入流
                        sw.WriteLine(memberData3.Count);
                        for (int count = 0; count < memberData3.Count; count++)
                        {
                            sw.WriteLine(memberData3[count].EMargin);
                            sw.WriteLine(memberData3[count].Value);
                        }

                        sw.WriteLine("测量时间：" + PatientInfo.finishtime);
                        sw.WriteLine("病人ID：" + PatientInfo.PatientId);
                        sw.WriteLine("病人床号：" + PatientInfo.PatientBed);
                        sw.WriteLine("病人性别：" + PatientInfo.PatientSex);

                        for (int count = 0; count < memberData3.Count; count++)
                        {
                            sw.WriteLine("第"+(count+1).ToString() + "测量点位置" + "(" + memberData3[count].Number + ")" + "," + "压力平均值" + memberData3[count].Value+"mmHg");
                        }
                        sw.Close(); //关闭文件
                        PatientInfo.IsSave = true;
                        PatientInfo.IsAddAndSavefailed = false;
                        PatientInfo.IsAdd = false;
                        PatientInfo.PatientData = null;
                        PatientInfo.readyornot = true;
                        this.Content = new AccessMain(false);
                    }
                    else
                    {
                        CMessageBoxResult confirmToDel = CMessageBox.Show("请选择病人进行添加测量！", CMessageBoxButton.OKCancel);
                    }

                }
                catch
                {
                    CMessageBox.Show("无信息保存！", CMessageBoxButton.OK);
                }
            }
        }

        #region 文件保存
        private void FileOpen(object sender, ExecutedRoutedEventArgs e)//打开文件快捷键事件crtl+O
        {
            OpenFileDialog open_fd = new OpenFileDialog();//调用系统打开文件窗口
            open_fd.Filter = "TXT文本|*.txt";//文件过滤器
            if (open_fd.ShowDialog() == true)//选择了文件
            {
                //sendTBox.Text = File.ReadAllText(open_fd.FileName);//读TXT方法1 简单，快捷，为StreamReader的封装
                //StreamReader sr = new StreamReader(open_fd.FileName);//读TXT方法2 复杂，功能强大
                //sendTBox.Text = sr.ReadToEnd();//调用ReadToEnd方法读取选中文件的全部内容
                //sr.Close();//关闭当前文件读取
            }
        }

        private void FileSave(object sender, RoutedEventArgs e)//保存数据按钮crtl+S
        {
            //SaveModWindow SaveMod = new SaveModWindow();//new保存数据方式窗口
            //SaveMod.Owner = this;//赋予主窗口，子窗口打开后，再次点击主窗口，子窗口闪烁
            //SaveMod.ShowDialog();//ShowDialog方式打开保存数据方式窗口
            //if (SaveMod.mode == "new")//保存为新文件
            //{
            //    SaveNew();//保存为新文件
            //}
            //else if (SaveMod.mode == "old")//保存到已有文件
            //{
            //    SaveOld();//保存到已有文件
            //}
            //else//取消
            //{
            //    return;
            //}
        }

        private void SaveNew_Click(object sender, RoutedEventArgs e)//文件-保存-保存为新文件click事件
        {
            SaveNew();//保存为新文件
        }

        private void SaveOld_Click(object sender, RoutedEventArgs e)//文件-保存-保存到已有文件click事件
        {
            SaveOld();//保存到已有文件
        }

        private void SaveNew()//保存为新文件
        {

            SaveFileDialog Save_fd = new SaveFileDialog();//调用系统保存文件窗口
            Save_fd.Filter = "TXT文本|*.txt";//文件过滤器
            if (Save_fd.ShowDialog() == true)//选择了文件
            {
                File.AppendAllText(Save_fd.FileName, "\r\n------" + DateTime.Now.ToString() + "\r\n");//数据后面写入时间戳
                MessageBox.Show("保存成功！");
            }
        }

        private void SaveOld()//保存到已有文件
        {

            OpenFileDialog Open_fd = new OpenFileDialog();//调用系统保存文件窗口
            Open_fd.Filter = "TXT文本|*.txt";//文件过滤器
            if (Open_fd.ShowDialog() == true)//选择了文件
            {
                File.AppendAllText(Open_fd.FileName, "\r\n------" + DateTime.Now.ToString() + "\r\n");//数据后面写入时间戳
                CMessageBox.Show("添加成功！");
            }
        }

        #endregion
        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {

        }
        private void checkBox2_Click(object sender, RoutedEventArgs e)
        {

        }
        //双击全选
        bool Dragable = true;//控件是否可以拖动
        bool isDragDropInEffect = false;
        Point pos = new Point();
        void Element_MouseMove(object sender, MouseEventArgs e)
        {
            if (isDragDropInEffect)
            {
                FrameworkElement currEle = sender as FrameworkElement;
                double xPos = e.GetPosition(null).X - pos.X + currEle.Margin.Left;
                double yPos = e.GetPosition(null).Y - pos.Y + currEle.Margin.Top;

                //判断拖动范围
                if (xPos >= 437)
                    xPos = 437;
                if (xPos <= 10)
                    xPos = 10;
                if (yPos >= 390)
                    yPos = 390;
                if (yPos <= 10)
                    yPos = 10;

                currEle.Margin = new Thickness(xPos, yPos, 0, 0);
                pos = e.GetPosition(null);
            }
        }
        void Element_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (Dragable) //可不可以拖动
            {
                FrameworkElement fEle = sender as FrameworkElement;
                isDragDropInEffect = true;
                pos = e.GetPosition(null);
                fEle.CaptureMouse();
                fEle.Cursor = Cursors.Hand;
            }
        }

        void Element_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (isDragDropInEffect)
            {
                FrameworkElement ele = sender as FrameworkElement;
                isDragDropInEffect = false;
                ele.ReleaseMouseCapture();
            }
        }

        private void SmallRange_Checked(object sender, RoutedEventArgs e)
        {
            bigORsmall = 1;
        }

        private void BigRange_Checked(object sender, RoutedEventArgs e)
        {
            bigORsmall = 0;
        }



    }

    public class Member3
    {
        public string Number { get; set; }  //位置
        public string Value { get; set; }//平均值
        public string EMargin { get; set; }//EMargin
    }//数据类
}


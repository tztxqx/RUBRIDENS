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


namespace RUBRIDENSii
{
    /// <summary>
    /// MeasureMain.xaml 的交互逻辑
    /// </summary>
    public partial class MeasureMain : UserControl
    {
        public MeasureMain()
        {
            InitializeComponent();
            for (int i = 0; i < 8; i++) //List存入各点数据
            {
                dataSource.Add(new ObservableDataSource<Point>());
            }
            for (int j = 0; j < 8; j++)  //List存入折线图
            {
                graph.Add(new LineGraph());
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
            ellipse.Add(epoint1);
            ellipse.Add(epoint2);
            ellipse.Add(epoint3);
            ellipse.Add(epoint4);
            ellipse.Add(epoint5);
            ellipse.Add(epoint6);
            ellipse.Add(epoint7);
            ellipse.Add(epoint8);
            #endregion

            #region checkbox
            checkbox.Add(checkBox1);
            checkbox.Add(checkBox2);
            checkbox.Add(checkBox3);
            checkbox.Add(checkBox4);

            checkbox.Add(checkBox11);
            checkbox.Add(checkBox22);
            checkbox.Add(checkBox33);
            checkbox.Add(checkBox44);
            #endregion

            if (PatientInfo.IsAdd == true)
                PatientInfo.IsAddAndSavefailed = true;
        }

        #region 属性
        SerialPort ComPort = new SerialPort();//声明一个串口      
        private string[] ports;//可用串口数组

        private bool ComPortIsOpen = false;//COM口开启状态字，在打开/关闭串口中使用，这里没有使用自带的ComPort.IsOpen，因为在串口突然丢失的时候，ComPort.IsOpen会自动false，逻辑混乱
        IList<customer> comList = new List<customer>();//可用串口集合
        private int ReadyOrStart=0;//用来判断是ready还是Start 0表示从未开启，1表示已经准备过了，2表示Start状态
        private string shujvbao = " ";  //数据传输string
        private int currentSecond = 0; //当前时间的值
        private int timelimit;  //时间长度界面上可以更改5s,10s,15s;

        private Color[] colorSelect = new Color[9]; //线条9种颜色
        
        public byte[] setiao = { 0, 0, 143, 0, 0, 207, 0, 16, 255, 0, 80, 255, 0, 143, 255, 0, 207, 255, 16, 255, 239, 80, 255, 175, 143, 255, 111, 207, 255, 48, 255, 239, 0, 255, 175, 0, 255, 111, 0, 255, 48, 0, 239, 0, 0, 191, 0, 0 };//热力颜色
        
        //dataSource
        private List<ObservableDataSource<Point>> dataSource = new List<ObservableDataSource<Point>>();
        private List<LineGraph> graph = new List<LineGraph>();
        private List<CheckBox> checkbox = new List<CheckBox>();

        private List<Ellipse> ellipse = new List<Ellipse>();

        #endregion


        public int leftRight=0;
        public int finalLR = 0;
        public int save = 0;
        public int havegot = 0;

        private void Window_Loaded(object sender, RoutedEventArgs e)//主窗口初始化
        {
            #region 可用串口下拉控件↓↓↓↓↓↓↓↓↓
            ports= SerialPort.GetPortNames();//获取可用串口
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
            for (int i = 0; i < 8; i++)
            {
                if(i<4)
                    graph[i] = plotter.AddLineGraph(dataSource[i], colorSelect[i], 2, Convert.ToString(i));
                else
                    graph[i] = plotter.AddLineGraph(dataSource[i], colorSelect[i%9], 2, Convert.ToString(i));
            }
            plotter.LegendVisible = false;
            plotter.Viewport.FitToView();
            #endregion
            ComPort.DataReceived += new SerialDataReceivedEventHandler(ComReceive);//串口接收中断
        }

        private void timerTick(string buffer,int LorR, int LR)
        {
            double x = currentSecond;
            double y;
            List < Point > point= new List<Point>();
            int LRbase = 0;
            int flag = 0;
            if (LorR == 1 && LR == 1)
            {
                LRbase = 0;
                flag = 1;
                finalLR = 1;
            }
            if (LorR == -1 && (LR == 2))
            {
                LRbase = 4;
                flag = 1;
                finalLR = -1;
            }
            if(flag ==1)
            {
                for (int i = 0; i < 4; i++)
                {
                    y = biaoding(buffer.Substring(i*4, 4),i);
                    point.Add(new Point(x, y));
                    dataSource[LRbase+i].AppendAsync(base.Dispatcher, point[i]);
                }
                currentSecond++;
            }
            else
            {

            }
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
            finalLR = 0;
            save = 0;
            leftRight = 0;
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
                timelimit = Convert.ToInt16(timeSelect.Text)*2-1;
                PatientInfo.timelimit = timelimit;
                try//尝试打开串口
                {
                    ComPort.PortName = AvailableComCbobox.SelectedValue.ToString();//设置要打开的串口
                    ComPort.BaudRate = 115200;//设置当前波特率
                    ComPort.Parity = 0;//设置当前校验位
                    ComPort.DataBits = 8;//设置当前数据位
                    ComPort.StopBits = (StopBits)1;//设置当前停止位  
                    leftRight = 0;
                    save = 0;
                    ComPort.Open();//打开串口
                    ReadyOrStart = 2; //设置指示
                    PatientInfo.readyornot = false;
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
                    leftRight = 0;
                    ReadyOrStart = 0; //设置指示
                    openBtn.Content = "开始测量";
                    PatientInfo.readyornot = true;
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
            }
            #endregion
        }
        private void ready_Button_Click(object sender, RoutedEventArgs e)//准备按钮
        {
            finalLR = 0;
            save = 0;
            leftRight = 0;
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
                    leftRight = 0;
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
                leftRight = 0;
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
                    leftRight = 0;
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
            }
            #endregion
        }

        private void Reset()  //取消准备后重置
        {
            currentSecond = 0;
            for (int i = 0; i < 8; i++)
            {
                plotter.Children.Remove(graph[i]);
            }
            dataSource.Clear();
            graph.Clear();
            for (int i = 0; i < 8; i++)
            {
                dataSource.Add(new ObservableDataSource<Point>());
                graph.Add(new LineGraph());
                if (i < 4)
                    graph[i] = plotter.AddLineGraph(dataSource[i], colorSelect[i], 2, Convert.ToString(i));
                else
                    graph[i] = plotter.AddLineGraph(dataSource[i], colorSelect[i % 4], 2, Convert.ToString(i));
            }
            plotter.LegendVisible = false;
            plotter.Viewport.FitToView();
            recCount.Text = "0"; //接收计数
            leftRight = 0;
        }

        private void ComReceive(object sender, SerialDataReceivedEventArgs e)//接收数据 中断只标志有数据需要读取，读取操作在中断外进行
        {
            Thread.Sleep(20);//发送和接收均为文本时，接收中为加入判断是否为文字的算法，发送你（C4E3），接收可能识别为C4,E3，可用在这里加延时解决
            if (ComPortIsOpen)//如果已经开启接收
            {
                byte[] recBufferd;//接收缓冲区
                try
                {
                    if (ComPort.BytesToRead >= 20)//缓存数据包中的内容
                    {
                        recBufferd = new byte[ComPort.BytesToRead];//接收数据缓存大小
                        ComPort.Read(recBufferd, 0, recBufferd.Length);//读取数据 
                        int start = 0;
                        for (; start < recBufferd.Length; start++)
                        {
                            if ((int)recBufferd[start] == 83)  //判断是否为S
                                break;
                        }
                        byte[] recBuffer = new byte[20];
                        for (int i = 0; i < 20; i++)
                            recBuffer[i] = recBufferd[start++];
                        #region 判断是否合格的数据，并进行显示
                        try
                        {
                            string recData;//接收数据转码后缓存
                            int LorR = 0; //判断左右脚
                            recData = System.Text.Encoding.Default.GetString(recBuffer);//转码
                            #region 数据包判断并显示
                            if (recData[0] == 'S' && recData.Length == 20 && recData[19]=='E') //根据协议处理，数据包头为00
                            {
                                shujvbao = "";
                                if (recData[1] == '0' && recData[2] == '0' && leftRight == 0)
                                {
                                    LorR = 1;//左脚
                                    finalLR = 1;
                                    leftRight = 1;
                                    shujvbao = recData.Substring(3, 16);
                                }
                                else if (recData[1] == '3' && recData[2] == '4' && leftRight == 1)
                                {
                                    LorR = -1;//右脚
                                    finalLR = -1;
                                    leftRight = 2;
                                    shujvbao = recData.Substring(3, 16);
                                }
                                else if (recData[1] == '0' && recData[2] == '0' && leftRight == 2)
                                {
                                    LorR = 1;
                                    finalLR = 1;
                                    leftRight = 1;
                                    shujvbao = recData.Substring(3, 16);
                                }
                                else
                                    LorR = 0;
                                
       
                                // Console.WriteLine(shujvbao);
                                #region 显示部分

                                #region 文字\数据显示
                                Task tasknew = Task.Run
                                    //UIAction
                                     (() => timerTick(shujvbao, LorR, leftRight));
                                tasknew.Wait();      
                                UIAction(() =>
                                            {
                                                if (LorR == 1 && finalLR==1)
                                                {
                                                    checkBox1.Content = "第一跖骨" + "(" + biaoding(shujvbao.Substring(0 * 4, 4), 0) + ")";
                                                    checkBox2.Content = "第三跖骨" + "(" + biaoding(shujvbao.Substring(1 * 4, 4), 1) + ")";
                                                    checkBox3.Content = "第五跖骨" + "(" + biaoding(shujvbao.Substring(2 * 4, 4), 2) + ")";
                                                    checkBox4.Content = "跟骨" + "(" + biaoding(shujvbao.Substring(3 * 4, 4), 3) + ")";
                                                }
                                                if (LorR == -1 && finalLR==-1)
                                                {
                                                    checkBox11.Content = "第一趾骨" + "(" + biaoding(shujvbao.Substring(0 * 4, 4), 4) + ")";
                                                    checkBox22.Content = "第三跖骨" + "(" + biaoding(shujvbao.Substring(1 * 4, 4), 5) + ")";
                                                    checkBox33.Content = "第五跖骨" + "(" + biaoding(shujvbao.Substring(2 * 4, 4), 6) + ")";
                                                    checkBox44.Content = "跟骨" + "(" + biaoding(shujvbao.Substring(3 * 4, 4),7) + ")";
                                                }
                                                recCount.Text = (Convert.ToInt32(recCount.Text) + recBufferd.Length).ToString();//接收数据字节数
                                            });
                                #endregion


                                #region 控件更改
                                UIAction(() =>
                                            {
                                                int LRbase = 0;
                                                int flag = 0;
                                                if (LorR == 1 && finalLR == 1)
                                                {
                                                    LRbase = 0;
                                                    flag = 1;

                                                }
                                                if (LorR == -1 && finalLR == -1)
                                                {
                                                    LRbase = 4;
                                                    flag = 1;
                                                }
                                                if (flag == 1)
                                                {
                                                    for (int ii = 0; ii < 4; ii++)
                                                    {
                                                        int temp = string2int(shujvbao.Substring(ii * 4, 4), ii);
                                                        ellipse[LRbase + ii].Fill = new SolidColorBrush(Color.FromRgb(setiao[temp * 3], setiao[temp * 3 + 1], setiao[temp * 3 + 2]));
                                                        ellipse[LRbase + ii].ToolTip = biaoding(shujvbao.Substring(ii * 4, 4), ii);
                                                    }
                                                    #region checkBox
                                                    for (int jj = 0; jj < 8; jj++)
                                                    {
                                                        if (checkbox[jj].IsChecked == true)
                                                        {
                                                            graph[jj].Visibility = Visibility.Visible;
                                                            ellipse[jj].Stroke = new SolidColorBrush(Colors.Red);
                                                        }
                                                        else
                                                        {
                                                            graph[jj].Visibility = Visibility.Hidden;
                                                            ellipse[jj].Stroke = new SolidColorBrush(Colors.White);
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                }
                                                #endregion

                                            });

                                #region ReadyOrStart==2 显示测量
                                if (ReadyOrStart == 2 ) //如果为测量模式
                                {
                                    UIAction(() =>
                                    {
                                        timeSlider.Maximum = timelimit; //进度条
                                        timeSlider.Value = currentSecond;//进度条
                                        if ((currentSecond > timelimit) && (ReadyOrStart == 2)) //ComPortIsOpen == true,当前串口为打开状态，按钮事件为关闭串口
                                        {

                                            try//尝试关闭串口
                                            {
                                                ComPort.DiscardInBuffer();//清接收缓存
                                                SetAfterClose();//成功关闭串口或串口丢失后的设置
                                                ComPort.Close();//关闭串口
                                                ReadyOrStart = 0; //设置指示
                                                CMessageBoxResult confirmToDel = CMessageBox.Show("测量完成", CMessageBoxButton.OK);
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

                                        if (save == 0 && finalLR == 1)
                                        {
                                            PatientInfo.PatientData += shujvbao; //记录当前保存的数据
                                            save = 1;
                                        }
                                        else if (save == 1 && finalLR == -1)
                                        {
                                            PatientInfo.PatientData += shujvbao; //记录当前保存的数据
                                            save = 2;
                                        }
                                        else if (save == 2 && finalLR == 1)
                                        {
                                            PatientInfo.PatientData += shujvbao; //记录当前保存的数据
                                            save = 1;
                                        }

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

        public double biaoding(string data,int i) //标定函数
        {
            
            double a = Convert.ToDouble(data);
            double b=a;
            if (i == 0)
            {
                if (a < 10)
                    b = 0;
                else
                    b =0.0517* a*2.125;
            }
            if (i == 1)
            {
                if (a <10)
                    b = 0;
                else
                    b = 0.0446*a;
            }
            if (i == 2)
            {
                if (a < 10)
                    b = 0;
                else
                    b =0.0369* a;
            }
            if (i == 3)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0616 * a * 1.545;
            }
            if (i == 4)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0267*a;
            }
            if (i == 5)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0467 * a;
            }
            if (i == 6)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0167 * a*1.1;
            }
            if (i == 7)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0472 * a * 1.545;
            }




            int _b = (int)(b * 100);

            b = (double)_b / 100;           //保留两位有效数字


            return b;
        }

        public int string2int(string s,int i) //转为合适的颜色
        {
            double a = biaoding(s,i)/200 * setiao.Length;
            if (a > setiao.Length/3-1)
                a = setiao.Length/3 - 1;

             if(a<0)
            {
                a = 0;
            }
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
            try
            {
                Console.WriteLine(PatientInfo.PatientData);
                if (PatientInfo.IsAdd == true)
                {
                    if (System.IO.File.Exists("footshow/" + PatientInfo.NameToSave  + ".fot"))
                    {
                        System.IO.File.Delete("footshow/" + PatientInfo.NameToSave  + ".fot");
                    }
                    FileStream fs = new FileStream("footshow/"+PatientInfo.NameToSave + ".fot", FileMode.OpenOrCreate, FileAccess.ReadWrite);
                    StreamWriter sw = new StreamWriter(fs); // 创建写入流
                    sw.WriteLine(PatientInfo.PatientData); // 写入Hello World

                    sw.WriteLine("测量时间：" + PatientInfo.finishtime);
                    sw.WriteLine("病人ID：" + PatientInfo.PatientId);
                    sw.WriteLine("病人床号：" + PatientInfo.PatientBed);
                    sw.WriteLine("病人性别：" + PatientInfo.PatientSex);

                    double[] average = new double[8];
                    for (int i = 0; i < 8; i++)
                    {
                        int timelimit = PatientInfo.PatientData.Length / 32;
                        for (int j = 0; j < timelimit; j++)
                        {
                            average[i] += biaoding(PatientInfo.PatientData.Substring(32* j + i*4, 4),i);
                            if (j == timelimit - 1)
                            {
                                average[i] = Math.Round(average[i] / timelimit, 2);
                                #region oh fuck
                                if (i == 0)
                                {
                                    sw.WriteLine();
                                    sw.WriteLine("左脚第一跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                }
                                if (i == 1)
                                    sw.WriteLine("左脚第三跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                if (i ==2)
                                    sw.WriteLine("左脚第五跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                if (i == 3)
                                {
                                    sw.WriteLine("左脚根骨" + "点压力的平均值：" + average[i] + "mmHg" );
                                    sw.WriteLine();
                                }
                                    
                                if (i == 4)
                                    sw.WriteLine("右脚第一跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                if (i == 5)
                                    sw.WriteLine("右脚第三跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                if (i == 6)
                                    sw.WriteLine("右脚第五跖骨" + "点压力的平均值：" + average[i] + "mmHg");
                                if (i == 7)
                                    sw.WriteLine("右脚跟骨" + "点压力的平均值：" + average[i] + "mmHg");

                                #endregion
                            }
                        }
                    }

                    sw.Close(); //关闭文件
                    PatientInfo.IsSave = true;
                    PatientInfo.IsAddAndSavefailed = false;
                    PatientInfo.IsAdd = false;
                    PatientInfo.PatientData = null;
                    PatientInfo.readyornot = true;
                    this.Content = new AccessMain(true);
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

        //双击全选
        private void TreeViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //checkBox1.IsChecked = true;
            //checkBox2.IsChecked = true;
            //checkBox3.IsChecked = true;
            //checkBox4.IsChecked = true;
        }

        private void treeRight_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            //checkBox11.IsChecked = true;
            //checkBox22.IsChecked = true;
            //checkBox33.IsChecked = true;
            //checkBox44.IsChecked = true;
        }

        #region checkBox click事件
        private void checkBox1_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox1.IsChecked == true)
                ellipse[0].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[0].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox2_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox2.IsChecked == true)
                ellipse[1].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[1].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox3_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox3.IsChecked == true)
                ellipse[2].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[2].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox4_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox4.IsChecked == true)
                ellipse[3].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[3].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox11_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox11.IsChecked == true)
                ellipse[4].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[4].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox22_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox22.IsChecked == true)
                ellipse[5].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[5].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox33_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox33.IsChecked == true)
                ellipse[6].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[6].Stroke = new SolidColorBrush(Colors.White);
        }

        private void checkBox44_Click(object sender, RoutedEventArgs e)
        {
            if (checkBox44.IsChecked == true)
                ellipse[7].Stroke = new SolidColorBrush(Colors.Red);
            else
                ellipse[7].Stroke = new SolidColorBrush(Colors.White);
        }
        #endregion



    }
}


using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
using System.IO;
using System.Windows.Threading;

namespace RUBRIDENSii
{
    /// <summary>
    /// FootShow.xaml 的交互逻辑
    /// </summary>
    public partial class FootShow : UserControl
    {
        public List<String> CurrentData = new List<string>();//存储当前病人的压力数据 数据格式：123456789123456789
        public int timelimit;
        public byte[] setiao = { 0, 0, 143, 0, 0, 207, 0, 16, 255, 0, 80, 255, 0, 143, 255, 0, 207, 255, 16, 255, 239, 80, 255, 175, 143, 255, 111, 207, 255, 48, 255, 239, 0, 255, 175, 0, 255, 111, 0, 255, 48, 0, 239, 0, 0, 191, 0, 0 };//热力颜色

        public string[] edata; //存储每个时间点的各点值
        private List<Ellipse> ellipse = new List<Ellipse>();//各个受力采集点集合
        public DispatcherTimer timer = new DispatcherTimer();

        public int count = 0;

        public FootShow(string PatientFile)
        {
            InitializeComponent();

            timer.Interval = TimeSpan.FromSeconds(0.8);   //设定时间0.4秒
            timer.Tick += new EventHandler(timer_Tick); //随时间播放

            using (StreamReader sr = new StreamReader("footshow/"+PatientFile + ".fot", Encoding.GetEncoding("utf-8"), true))
            {
                string data;
                while (!sr.EndOfStream)
                {
                    data = sr.ReadLine();
                    CurrentData.Add(data);

                    count++;
                }
                sr.Close();
            }

            timelimit = CurrentData[0].Length / 32;
            edata = new string[timelimit]; //生成对应时间点的压力字符串

            for (int i = 0; i < timelimit; i++)
            {
                edata[i] = CurrentData[0].Substring(i * 32, 32);
            }

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

            for (int i = 1; i < count; i++)
            {
                patientinfoLabel.AppendText(CurrentData[i] + "\r");
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            if (CurrentData != null)
                timeSlider.Maximum = timelimit - 1; //动态设置滚动条长度
        }

        private void timeSlider_MouseMove(object sender, MouseEventArgs e)
        {
            int time = (int)timeSlider.Value;
            for (int ii = 0; ii < 8; ii++)
            {
                int temp = string2int(edata[time].Substring(ii * 4, 4), ii);
                ellipse[ii].Fill = new SolidColorBrush(Color.FromRgb(setiao[temp * 3], setiao[temp * 3 + 1], setiao[temp * 3 + 2]));
                ellipse[ii].ToolTip = biaoding(edata[time].Substring(ii * 4, 4), ii);
            }

            checkBox1.Content = "第一跖骨" + "(" + biaoding(edata[time].Substring(0 * 4, 4), 0) + ")";
            checkBox2.Content = "第三跖骨" + "(" + biaoding(edata[time].Substring(1 * 4, 4), 1) + ")";
            checkBox3.Content = "第五跖骨" + "(" + biaoding(edata[time].Substring(2 * 4, 4), 2) + ")";
            checkBox4.Content = "跟骨" + "(" + biaoding(edata[time].Substring(3 * 4, 4), 3) + ")";
            checkBox11.Content = "第一跖骨" + "(" + biaoding(edata[time].Substring(4 * 4, 4), 4) + ")";
            checkBox22.Content = "第三跖骨" + "(" + biaoding(edata[time].Substring(5 * 4, 4), 5) + ")";
            checkBox33.Content = "第五跖骨" + "(" + biaoding(edata[time].Substring(6 * 4, 4), 6) + ")";
            checkBox44.Content = "跟骨" + "(" + biaoding(edata[time].Substring(7 * 4, 4), 7) + ")";
        }

        public double biaoding(string data, int i) //标定函数
        {

            double a = Convert.ToDouble(data);
            double b = a;
            if (i == 0)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0517 * a * 2.125;
            }
            if (i == 1)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0446 * a;
            }
            if (i == 2)
            {
                if (a < 10)
                    b = 0;
                else
                    b = 0.0369 * a;
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
                    b = 0.0267 * a;
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
                    b = 0.0167 * a * 1.1;
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


        public int string2int(string s, int i) //转为合适的颜色
        {
            double a = biaoding(s, i) / 200 * setiao.Length;
            if (a > setiao.Length / 3 - 1)
                a = setiao.Length / 3 - 1;
            return Convert.ToInt16(a);
        }

        void timer_Tick(object sender, EventArgs e)//随时间播放
        {
            if (timeSlider.Value < timeSlider.Maximum)
            {
                timeSlider.Value += 1;
                int time = (int)timeSlider.Value;
                BrushConverter brushConverter = new BrushConverter();
                for (int ii = 0; ii < 8; ii++)
                {
                    int temp = string2int(edata[time].Substring(ii * 4, 4), ii);
                    ellipse[ii].Fill = new SolidColorBrush(Color.FromRgb(setiao[temp * 3], setiao[temp * 3 + 1], setiao[temp * 3 + 2]));
                    ellipse[ii].ToolTip = biaoding(edata[time].Substring(ii * 4, 4), ii);
                }

                if (timeSlider.Value == timeSlider.Maximum)
                {
                    timer.Stop();
                    ifplaying = 0;
                    playButton.Content = "重新播放";
                }

                checkBox1.Content = "第一跖骨" + "(" + biaoding(edata[time].Substring(0 * 4, 4), 0) + ")";
                checkBox2.Content = "第三跖骨" + "(" + biaoding(edata[time].Substring(1 * 4, 4), 1) + ")";
                checkBox3.Content = "第五跖骨" + "(" + biaoding(edata[time].Substring(2 * 4, 4), 2) + ")";
                checkBox4.Content = "跟骨" + "(" + biaoding(edata[time].Substring(3 * 4, 4), 3) + ")";

                checkBox11.Content = "第一跖骨" + "(" + biaoding(edata[time].Substring(4 * 4, 4), 4) + ")";
                checkBox22.Content = "第三跖骨" + "(" + biaoding(edata[time].Substring(5 * 4, 4),5) + ")";
                checkBox33.Content = "第五跖骨" + "(" + biaoding(edata[time].Substring(6 * 4, 4), 6) + ")";
                checkBox44.Content = "跟骨" + "(" + biaoding(edata[time].Substring(7 * 4, 4),7) + ")";
            }
            if (timeSlider.Value == timeSlider.Maximum)
                timer.Stop();
        }
        public int ifplaying = 0;//判断是否播放中
        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            if (ifplaying == 0)
            {
                playButton.Content = "停止播放";
                if (timeSlider.Value == timeSlider.Maximum)  //播放完成重新播放
                    timeSlider.Value = 0;
                timer.Start();
                ifplaying = 1;
                if (timeSlider.Value == timeSlider.Maximum)
                {
                    timer.Stop();
                    ifplaying = 0;
                    playButton.Content = "重新播放";
                }
            }
            else
            {
                playButton.Content = "播放";
                ifplaying = 0;
                timer.Stop();
            }
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

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)patientinfoLabel.Document).DocumentPaginator, "A Flow Document");
            }
        }

    }
}

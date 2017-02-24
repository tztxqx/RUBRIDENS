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
using MahApps.Metro.Controls;

namespace RUBRIDENSii
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : MetroWindow
    {
        private ContentControl s = new ContentControl();
        private ContentControl single = new ContentControl();
        private ContentControl footData = new ContentControl();
        //private AccessMain singleAccess=null;
        //private AccessMain footAccess = null;

        
        
        public MainWindow()
        {
            InitializeComponent();
            s.Content = contentControl1.Content;
            single.Content = new AccessMain(false);
            footData.Content = new AccessMain(true);
        }


        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            contentControl1.Content = s.Content;
        }

        private void DirectMeasure_Click(object sender, RoutedEventArgs e)
        {
//             contentControl1.Content = new Single();
        }

        //足底数据直接测量
        private void Displaybutton_Click(object sender, RoutedEventArgs e)
        {
            if (PatientInfo.readyornot)
            {
                PatientInfo.loadornot = false;
                contentControl1.Content = new AccessMain(true);
                //contentControl1.Content = footData.Content;
            }
            else
            {
                CMessageBox.Show("请完成测量后再跳转至其它页面", CMessageBoxButton.OK);
            }
        }

        //单点式数据直接浏览
        private void DispalyDandian_Click(object sender, RoutedEventArgs e)
        {
            if (PatientInfo.readyornot)
            {


                PatientInfo.loadornot = false;

                contentControl1.Content = null;
                contentControl1.Content = new AccessMain(false);


                //contentControl1.Content = single.Content;
            }
            else
            {
                CMessageBox.Show("请完成测量后再跳转至其它页面", CMessageBoxButton.OK);
            }

        }

        private void MetroWindow_Closed(object sender, EventArgs e)
        {
            Application.Current.Shutdown();//先停止线程,然后终止进程.
            Environment.Exit(0);//直接终止进程.
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)//联系我们
        {
            CMessageBox.Show("医疗压力测量系统v1.0,请联系18810913505", CMessageBoxButton.OK);
        }

        //登录单点式
        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            PatientInfo.PatientName = TextBox6.Text;
            PatientInfo.PatientId = TextBox7.Text;
            if (PatientInfo.PatientId.Length != 8)
            {
                CMessageBox.Show("病历号需为八位！", CMessageBoxButton.OK);
            }
            else
            {
                PatientInfo.loadornot = true;
                contentControl1.Content = new AccessMain(false);
            }
        }

        //登录足底病历
        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            PatientInfo.PatientName = TextBox6.Text;
            PatientInfo.PatientId = TextBox7.Text;
            if (PatientInfo.PatientId.Length != 8)
            {
                CMessageBox.Show("病历号需为八位！", CMessageBoxButton.OK);
            }
            else
            {
                PatientInfo.loadornot = true;
                contentControl1.Content = new AccessMain(true);
            }
        }

        //添加足底病历
        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            PatientInfo.PatientName = TextBox1.Text;
            PatientInfo.PatientId = TextBox2.Text;
            PatientInfo.PatientBed = TextBox3.Text;
            PatientInfo.PatientSex = TextBox4.Text;
            PatientInfo.PatientAge = TextBox5.Text;
            if (PatientInfo.PatientId.Length != 8)
            {
                CMessageBox.Show("病历号需为八位！", CMessageBoxButton.OK);
            }
            else if (PatientInfo.PatientBed.Length == 0)
            {
                CMessageBox.Show("床号不能为零！", CMessageBoxButton.OK);
            }
            else
            {
                PatientInfo.newornot = true;
                PatientInfo.IsAdd = true;
                PatientInfo.IsAddAndSavefailed = true;
                PatientInfo.PatientIdToSave = TextBox2.Text;
                PatientInfo.NameToSave = TextBox2.Text + "1";
                PatientInfo.IsSave = false;
                contentControl1.Content = new MeasureMain();
            }
            
            //添加病人信息
            //id,床号,性别,病历号,姓名,年龄,最大次数,住院日期,次数,备注,发布日期,修改日期
            int a = 1;
            PatientInfo.Add[0] = TextBox3.Text;//床号
            PatientInfo.Add[1] = TextBox4.Text;//性别
            PatientInfo.Add[2] = TextBox2.Text;//病历号
            PatientInfo.Add[3] = TextBox1.Text;//姓名
            PatientInfo.Add[4] = TextBox5.Text;//年龄
            PatientInfo.Add[5] = a.ToString();//最大次数
            PatientInfo.Add[6] = DateTime.Now.ToString();
            PatientInfo.Add[7] = a.ToString();
            PatientInfo.Add[8] = "无";
            PatientInfo.Add[9] = DateTime.Now.ToString();
            PatientInfo.Add[10] = PatientInfo.Add[2] + PatientInfo.Add[7];
        }

        //添加单点病历
        private void Button_Click_5(object sender, RoutedEventArgs e)
        {
            PatientInfo.PatientName = TextBox1.Text;
            PatientInfo.PatientId = TextBox2.Text;
            PatientInfo.PatientBed = TextBox3.Text;
            PatientInfo.PatientSex = TextBox4.Text;
            PatientInfo.PatientAge = TextBox5.Text;
            if (PatientInfo.PatientId.Length != 8)
            {
                CMessageBox.Show("病历号需为八位！", CMessageBoxButton.OK);
            }
            else if (PatientInfo.PatientBed.Length == 0)
            {
                CMessageBox.Show("床号不能为零！", CMessageBoxButton.OK);
            }
            else
            {
                PatientInfo.newornot = true;
                PatientInfo.IsAdd = true;
                PatientInfo.IsAddAndSavefailed = true;
                PatientInfo.PatientIdToSave = TextBox2.Text;
                PatientInfo.NameToSave = TextBox2.Text + "1";
                PatientInfo.IsSave = false;
                contentControl1.Content = new Single();
            }
            //添加病人信息
            //id,床号,性别,病历号,姓名,年龄,最大次数,住院日期,次数,备注,发布日期,修改日期
            int a = 1;
            PatientInfo.Add[0] = TextBox3.Text;//床号
            PatientInfo.Add[1] = TextBox4.Text;//性别
            PatientInfo.Add[2] = TextBox2.Text;//病历号
            PatientInfo.Add[3] = TextBox1.Text;//姓名
            PatientInfo.Add[4] = TextBox5.Text;//年龄
            PatientInfo.Add[5] = a.ToString();//最大次数
            PatientInfo.Add[6] = DateTime.Now.ToString();
            PatientInfo.Add[7] = a.ToString();
            PatientInfo.Add[8] = "无";
            PatientInfo.Add[9] = DateTime.Now.ToString();
            PatientInfo.Add[10] = PatientInfo.Add[2] + PatientInfo.Add[7];
        }

    }
}

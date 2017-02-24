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
using System.Globalization;
using System.Collections.ObjectModel;

namespace RUBRIDENSii
{
    /// <summary>
    /// SingelShow.xaml 的交互逻辑
    /// </summary>
    public partial class SingleShow : UserControl
    {
        public SingleShow(string PatientFile)
        {

            #region 全局变量定义
            int PointNum;
            string Num = "";
            double Value = 0;
            string Emargin="";
            ObservableCollection<MemberSingle> memberPoint = new ObservableCollection<MemberSingle>();
            MemberSingle dataGrid_SelectedElement = null;



            List<Ellipse> MarkedCol = new List<Ellipse>();
            List<String> TextReader = new List<string>();
            #endregion

            InitializeComponent();

            using (StreamReader sr = new StreamReader("singleshow/"+PatientFile + ".fot", Encoding.GetEncoding("utf-8"), true))
            {
                string data;
                data =sr.ReadLine();
                PointNum = Convert.ToInt32(data); //存入点的个数

                #region 读取点并存入可观测集合并添加到图中显示
                for (int i = 0; i < PointNum;i++ )
                {
                    data = sr.ReadLine();
                    Emargin = data;
                    data = sr.ReadLine();
                    Value = Convert.ToDouble(data);

                    string NumTemp = Emargin;
                    int NumLen = 0;
                    for (int commNum = 0; commNum < 2; NumLen++)
                    {
                        if (NumTemp[NumLen] == ',')
                            commNum += 1;
                    }
                    Num = NumTemp.Substring(0, NumLen - 1);

                    #region 在grid中添加数据
                    //添加新的数据项

                    memberPoint.Add(new MemberSingle()
                    {//Name病历号 Age姓名
                        Number = Num.ToString(),
                        Value = Value.ToString(),
                        EMargin = Emargin.ToString(),
                    });
                    #region 标记ellipse
                    //标记新的ellipse
                    Ellipse MarkedPoint = new Ellipse();
                    MarkedCol.Add(MarkedPoint);
                    MarkedPoint.VerticalAlignment = VerticalAlignment.Top;
                    MarkedPoint.HorizontalAlignment = HorizontalAlignment.Left;
                    MarkedPoint.Width = 10;
                    MarkedPoint.Height = 10;
                    double left=0;double top=0;
                    string leftTemp="",topTemp="";
                    //将margin值给出
                    for(int j=0,comm=0;j<Num.Length;j++)
                    {
                       if(Num[j]==',')
                       {
                           comm+=1;
                           continue;
                       }
                        if(comm==0)
                        {
                            leftTemp+=Num[j];
                        }
                        else
                        {
                            topTemp+=Num[j];
                        }
                    }
                    left = Convert.ToDouble(leftTemp);
                    top = Convert.ToDouble(topTemp);

                    Thickness thick = new Thickness(left, top, 0, 0);
                    MarkedPoint.Margin = thick;//设置margin值
                    PersonPhoto.Children.Add(MarkedPoint);
                    MarkedPoint.Fill = new SolidColorBrush(Colors.Pink);
                    MarkedPoint.ToolTip = Value.ToString();

                    #endregion
                    dataGrid1.DataContext = memberPoint;
                    #endregion

                }
                #endregion
                while (!sr.EndOfStream)
                {
                    data = sr.ReadLine();
                    TextReader.Add(data);
                      // count++;
                }
                sr.Close();
            }
            for (int i = 0; i < TextReader.Count; i++)
            {
                patientinfoLabel.AppendText(TextReader[i] + "\r");
            }

        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {

        }

        private void printButton_Click(object sender, RoutedEventArgs e)
        {
            PrintDialog printDialog = new PrintDialog();
            if (printDialog.ShowDialog() == true)
            {
                printDialog.PrintDocument(((IDocumentPaginatorSource)patientinfoLabel.Document).DocumentPaginator, "A Flow Document");
            }
        }
    }
    public class MemberSingle
    {
        public string Number { get; set; }  //位置
        public string Value { get; set; }//平均值
        public string EMargin { get; set; }//EMargin
    }//数据类
}

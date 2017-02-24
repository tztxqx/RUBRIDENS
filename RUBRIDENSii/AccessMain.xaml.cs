using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using DataProcessingTool;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows.Forms;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Globalization;


namespace RUBRIDENSii
{
    /// <summary>
    /// AccessMain.xaml 的交互逻辑
    /// </summary>

    public partial class AccessMain : System.Windows.Controls.UserControl, IDisposable
    {
        #region 属性及字段定义
        _DataClass HMSDATA = new _DataClass();  //数据库连接及保存用对象 详见_DataClass
        string FileName = ""; //文件名（不包含路径）
        DPT dpt;//数据处理对象 及数据储存对象（内嵌） 详见DPT
        DataTable Clipboard;//剪贴板
        DataTable Searchboard;//Search板
        DataTable Deleteboard;//删除板
        bool clipboardused = false;//剪贴板是否已经被使用
        bool _SearchboardOpen = false;//Search板是否开启
        bool SearchboardOpen
        {
            get
            {
                return _SearchboardOpen;
            }
            set
            {
                _SearchboardOpen = value;
            }
        }//Search板开启时的附加操作
        bool _Save = false;//文件是否已经保存
        bool Saved
        {
            get
            {
                return _Save;
            }
            set
            {
                if (value == true)
                {
                    _Save = value;
                }
                else
                {
                    _Save = value;
                }
            }
        }//文件保存年龄额外操作
        bool _Load = false;//文件是否已经读取
        bool HaveLoaded
        {
            get
            {
                return _Load;
            }
            set
            {
                if (value == true)
                    _Load = value;
                //IFCON(value);
            }
        }//文件读取年龄额外操作
        string[] final = new string[11];//确定后由final数组传出用户选项
        DateTime[] Date = new DateTime[4];//存储用户选择的时间，也用于传出用户的选择
        int[] SearchOpiton = { 1, 1, 1, 1 };
        #endregion

        #region 方法定义
        DataRow ANewDataRow(string[] a)
        {
            DataRow NewR = HMSDATA.GetData().Tables[0].NewRow();
            if (a[0] == "")
                NewR["id"] = -1;
            else
                NewR["id"] = Convert.ToInt32(a[0]);
            NewR["床号"] = a[1];
            NewR["性别"] = a[2];
            NewR["病历号"] = a[3];
            NewR["姓名"] = a[4];
            NewR["年龄"] = a[5];
            NewR["最大次数"] = a[6];
            NewR["住院日期"] = Convert.ToInt32(a[7]);
            NewR["次数"] = Convert.ToInt32(a[8]);
            NewR["备注"] = a[9];
            NewR["发布日期"] = a[10];
            NewR["修改日期"] = a[11];
            return NewR;
        }//用string生成datarow
        private void LoadFile()
        {
            try
            {
                HMSDATA.SetSheetName("HMSDATA");
                HMSDATA.Connect();
                HMSDATA.SheetCon();
                HMSDATA.Fill_DataSet();
                HaveLoaded = true;
                dpt = new DPT(HMSDATA.GetData().Tables[0].Clone());
                dpt.InitializeAL(HMSDATA.GetData());
                if (clipboardused == false)
                {
                    clipboardused = true;
                    Clipboard = HMSDATA.GetData().Tables[0].Clone();
                }
                Searchboard = HMSDATA.GetData().Tables[0].Clone();
                Deleteboard = HMSDATA.GetData().Tables[0].Clone();
            }
            catch
            {
                System.Windows.MessageBox.Show("数据库连接失败！");
            }
        }//读取
        private int min(int a, int b)
        {
            if (a < b)
                return a;
            return b;
        }//取最小
        private void fflushSearch(DataTable datatable)
        {
            Searchboard.Clear();
            int i = 0;
            while (i < datatable.Rows.Count)
            {
                //_Data.Tables[0].ImportRow(dataset.Tables[0].Rows[i]);
                {
                    DataRow NewR = Searchboard.NewRow();
                    NewR["id"] = datatable.Rows[i]["id"];
                    NewR["床号"] = datatable.Rows[i]["床号"];
                    NewR["性别"] = datatable.Rows[i]["性别"];
                    NewR["病历号"] = datatable.Rows[i]["病历号"];
                    NewR["姓名"] = datatable.Rows[i]["姓名"];
                    NewR["年龄"] = datatable.Rows[i]["年龄"];
                    NewR["最大次数"] = datatable.Rows[i]["最大次数"];
                    NewR["住院日期"] = datatable.Rows[i]["住院日期"];
                    NewR["次数"] = datatable.Rows[i]["次数"];
                    NewR["备注"] = datatable.Rows[i]["备注"];
                    NewR["发布日期"] = datatable.Rows[i]["发布日期"];
                    NewR["修改日期"] = datatable.Rows[i]["修改日期"];
                    Searchboard.Rows.Add(NewR);
                }
                i++;
            }
        }//刷新Search板
        #endregion

        #region 全局变量
        bool[] Check = new bool[100000000];
        bool IsForfoot = true;
        int choosed = 0;
        int SelectRowNum_Change = 0;
        Member dataGrid_SelectedElement = null;

        private MeasureMain mm = new MeasureMain();
        private Single ss = new Single();
        List<string> Numtoshow = new List<string>();
        #endregion

        public void Dispose()
        {
            //this.HMSDATA = null;
            //this.final = null;
            //this.Numtoshow.Clear();
            //this.mm = null;
            //this.ss = null;
            //this.SearchOpiton = null;
            //// 调用Dispose需要只是GC不要再调用Finalize（）.
            //GC.SuppressFinalize(this);
        }


        public AccessMain(bool Isfoot)
        {
            IsForfoot = Isfoot;
            InitializeComponent();
            HMSDATA.Clear();
            if (IsForfoot == true)
            {
                HMSDATA.SetPlace("TestData.accdb");
                FileName = "TestData.accdb";
            }
            else
            {
                HMSDATA.SetPlace("TestData2.accdb");
                FileName = "TestData2.accdb";
            }
            Saved = true;
            LoadFile();
            homeCtrlHToolStripMenuItem_Click();
            if (HMSDATA.IfCon == false)
            {
                CMessageBox.Show("连接失败！", CMessageBoxButton.OK);
            }
            DataGridView1.DataSource = HMSDATA.GetData().Tables[0];
            for (int i = 0; i < 12; i++)
            { DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable; DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; }
//            Addnewornot();
        }//初始化函数




        private void Loadnewornot()//判断是否需要添加新的项
        {
            if (PatientInfo.loadornot == true)
            {
                string[] Add = new string[12];
                Add[0] = null;
                Add[1] = null;
                Add[2] = PatientInfo.PatientId;//获得病历号
                Add[3] = PatientInfo.PatientName;//获得姓名
                Add[4] = null;
                Add[5] = null;
                Add[6] = null;
                Add[7] = null;
                Add[8] = null;
                Add[9] = null;
                Add[10] = null;
                fflushSearch(dpt.Search(Add, Date, SearchOpiton));
                SearchboardOpen = true;
                DataGridView1.DataSource = Searchboard;
                for (int i = 0; i < 12; i++)
                    DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (DataGridView1.RowCount == 0) CMessageBox.Show("未搜索到相关记录！", CMessageBoxButton.OK);

                //在表格1与表格2中分别显示记录
                ObservableCollection<Member> memberData = new ObservableCollection<Member>();
                memberData.Add(new Member()
                {//Name病历号 Age姓名
                    Name = Add[2],
                    Age = Add[3]
                });
                dataGrid.DataContext = memberData;
                LoadGrid2();
//                PatientInfo.PatientId = null;
//                PatientInfo.PatientName = null;
//                PatientInfo.loadornot = false;
            }
        }
        private void Addnewornot()//判断是否需要添加新的项
        {
            if (PatientInfo.newornot == true)
            {
                //床号,性别,病历号,姓名,年龄,最大次数,住院日期,次数,备注,发布日期,修改日期
                string[] Add = new string[11];
                Add[0] = PatientInfo.PatientBed;
                Add[1] = PatientInfo.PatientSex;
                Add[2] = PatientInfo.PatientId;
                Add[3] = PatientInfo.PatientName;
                Add[4] = PatientInfo.PatientAge;
                Add[5] = "1";
                Add[6] = "1";
                Add[7] = "1";
                Add[8] = "无";
                Add[9] = DateTime.Now.ToString();
                Add[10] = Add[2] + Add[7];
                HMSDATA.NewRow(Add);
                PatientInfo.PatientName = null;//还原初始值
                PatientInfo.PatientId = null;
                PatientInfo.PatientBed = null;
                PatientInfo.PatientSex = null;
                PatientInfo.PatientAge = null;
                PatientInfo.newornot = false;
                HMSDATA.Clear();
                if (IsForfoot == true)
                {
                    HMSDATA.SetPlace("TestData.accdb");
                    FileName = "TestData.accdb";
                }
                else
                {
                    HMSDATA.SetPlace("TestData2.accdb");
                    FileName = "TestData2.accdb";
                }
                LoadFile();
                DataGridView1.DataSource = HMSDATA.GetData().Tables[0];
                for (int i = 0; i < 12; i++)
                { DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable; DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; }
            }
        }
        private void LoadGrid1()
        {
            if (PatientInfo.IsSave == false)
            {
                ObservableCollection<Member> memberData = new ObservableCollection<Member>();
                for (int i = 0; i < DataGridView1.Rows.Count; i++)
                {
                    String stringName = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[3].Value.ToString();
                    int intName = Convert.ToInt32(stringName);
                    if (Check[intName] == false)
                    {
                        memberData.Add(new Member()
                        {//Name病历号 Age姓名
                            Name = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[3].Value.ToString(),
                            Age = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[4].Value.ToString()
                        });
                        Check[intName] = true;
                    }
                }
                dataGrid.DataContext = memberData;
                Array.Clear(Check, 0, Check.Length);
            }
            else
            {
                ObservableCollection<Member> memberData = new ObservableCollection<Member>();
                for (int i = 0, j = 0; i < DataGridView1.Rows.Count; i++)
                {
                    String stringName = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[3].Value.ToString();
                    int intName = Convert.ToInt32(stringName);
                    if (Check[intName] == false)
                    {
                        memberData.Add(new Member()
                        {//Name病历号 Age姓名
                            Name = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[3].Value.ToString(),
                            Age = DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[4].Value.ToString()
                        });
                        Check[intName] = true;
                        if (stringName == PatientInfo.PatientIdToSave)
                        {
                            PatientInfo.SelectRowNum = j;
                        }
                        j++;
                    }
                }
                dataGrid.DataContext = memberData;
                Array.Clear(Check, 0, Check.Length);
            }
        }//显示第一个表格中的项
        private void LoadGrid2()
        {
            ObservableCollection<Member2> memberData2 = new ObservableCollection<Member2>();
            Numtoshow.Clear();
            for (int i = 0; i < DataGridView1.Rows.Count; i++)
            {
                Numtoshow.Add(DataGridView1.Rows[DataGridView1.Rows[i].Index].Cells[8].Value.ToString());
                memberData2.Add(new Member2()
                {
                    Num = (i+1).ToString()
                });
            }
            dataGrid1.DataContext = memberData2;
        }//显示第二个表格中的项
        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            dataGrid_SelectedElement = (Member)dataGrid.SelectedItem;
            if (dataGrid_SelectedElement != null)
            {
                string result = dataGrid_SelectedElement.Name.ToString();
                choosed = Convert.ToInt32(result);

                Searchboard.Clear();
                final[0] = null;//床号
                final[1] = null;//性别
                final[2] = dataGrid_SelectedElement.Name.ToString();//病历号
                final[3] = dataGrid_SelectedElement.Age.ToString();//姓名
                final[4] = null;//年龄
                final[5] = null;//最大次数
                final[6] = null;//住院日期
                final[7] = null;//次数
                final[8] = null;//备注
                final[9] = null;//发布日期
                final[10] = null;//修改日期
                fflushSearch(dpt.Search(final, Date, SearchOpiton));
                SearchboardOpen = true;
                DataGridView1.DataSource = Searchboard;
                for (int i = 0; i < 12; i++)
                    DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                if (DataGridView1.RowCount == 0) CMessageBox.Show("未搜索到相关记录！", CMessageBoxButton.OK); ;
                LoadGrid2();
            }
        }//获得第一个表中的数据
        private void dataGrid1_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            try
            {
                Member SelectedElement = (Member)dataGrid.SelectedItem;
                Member2 SelectedElement1 = (Member2)dataGrid1.SelectedItem;
                if (SelectedElement != null && SelectedElement1 != null)
                {
                    string result1 = SelectedElement.Name.ToString();//获得当前病历号
                    //string result = SelectedElement1.Num.ToString();//获得该病历号的次数
                    int a=Convert.ToInt32(SelectedElement1.Num)-1;
                    string result = Numtoshow.ElementAt(a);//获得该病历号的次数
                    string NameToDispaly = result1 + result;//获得需要显示的文件名
                    //在这里添加显示的代码

                    //显示单点还是显示足底根据IsForfoot判断
                    if(IsForfoot)
                        ContentControlDisplay.Content = new FootShow(NameToDispaly);
                    else
                        ContentControlDisplay.Content = new SingleShow(NameToDispaly);
                }
            }
            catch
            {
            }
        }//获得第二个表中的数据
        private void addnew()
        {

        }//创建一个新的数据库
        private void Button_Click_1(object sender, RoutedEventArgs e)//直接测量按钮按下
        {
            Member SelectedElement = (Member)dataGrid.SelectedItem;
            if (SelectedElement == null)
            {
                CMessageBox.Show("请首先选择一个病人信息！", CMessageBoxButton.OK);
            }
            else
            {
                PatientInfo.SelectRowNum = dataGrid.SelectedIndex;
                HMSDATA.Connect();
                HMSDATA.SheetCon();
                //在数据库中添加记录
                //string[] Add = new string[11];
                PatientInfo.Add[0] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[1].Value.ToString();
                PatientInfo.Add[1] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[2].Value.ToString();
                PatientInfo.Add[2] = SelectedElement.Name.ToString();
                PatientInfo.Add[3] = SelectedElement.Age.ToString();
                PatientInfo.Add[4] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[5].Value.ToString();
                PatientInfo.Add[5] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[6].Value.ToString();
                PatientInfo.Add[6] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[7].Value.ToString();
                PatientInfo.Add[7] = (Convert.ToInt32(DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[8].Value.ToString()) + 1).ToString();
                PatientInfo.Add[8] = DataGridView1.Rows[DataGridView1.Rows[DataGridView1.Rows.Count - 1].Index].Cells[9].Value.ToString();
                PatientInfo.Add[9] = DateTime.Now.ToString();
                PatientInfo.Add[10] = PatientInfo.Add[2] + PatientInfo.Add[7];


                CMessageBoxResult confirmToDel = CMessageBox.Show("确定要添加测量吗？", CMessageBoxButton.OKCancel);
                if (confirmToDel == CMessageBoxResult.OK)
                {
                    //Do Nothing
                    if (dataGrid_SelectedElement != null)
                    {
                        string result = dataGrid_SelectedElement.Name.ToString();
                        choosed = Convert.ToInt32(result);

                        Searchboard.Clear();
                        final[0] = null;//床号
                        final[1] = null;//性别
                        final[2] = dataGrid_SelectedElement.Name.ToString();//病历号
                        final[3] = dataGrid_SelectedElement.Age.ToString();//姓名
                        final[4] = null;//年龄
                        final[5] = null;//最大次数
                        final[6] = null;//住院日期
                        final[7] = null;//次数
                        final[8] = null;//备注
                        final[9] = null;//发布日期
                        final[10] = null;//修改日期
                        fflushSearch(dpt.Search(final, Date, SearchOpiton));
                        SearchboardOpen = true;
                        DataGridView1.DataSource = Searchboard;
                        for (int i = 0; i < 12; i++)
                            DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        if (DataGridView1.RowCount == 0) CMessageBox.Show("未搜索到相关记录！", CMessageBoxButton.OK);
                        LoadGrid2();

                        PatientInfo.IsAdd = true;
                        PatientInfo.PatientIdToSave = PatientInfo.Add[2];
                        PatientInfo.NameToSave = PatientInfo.Add[10];
                        PatientInfo.PatientId = PatientInfo.Add[2];
                        PatientInfo.PatientBed = PatientInfo.Add[0];
                        PatientInfo.PatientSex = PatientInfo.Add[1];
                        if (IsForfoot)
                        {
                            this.Content = mm;
                        }
                        else
                        {
                            this.Content = ss;
                        }
                    }
                }
                else
                {
                    //Do Nothing
                }
            }
        }
        private void Button_Click_2(object sender, RoutedEventArgs e)//删除记录按钮按下
        {
            string NameToDelete = null;
            string NameToDelete1 = null;
            bool DetelteAll = false;
            int max_delete = 0;
            bool delete = false;
            Member SelectedElement = (Member)dataGrid.SelectedItem;
            Member2 SelectedElement1 = (Member2)dataGrid1.SelectedItem;
            if (SelectedElement == null)
            {
                CMessageBox.Show("请首先选择一个病人信息！", CMessageBoxButton.OK);
            }
            else
            {
                if (SelectedElement1 == null)
                {
                    CMessageBoxResult confirmToDel = CMessageBox.Show("确定要删除该病人的所有记录吗？", CMessageBoxButton.OKCancel);
                    if (confirmToDel == CMessageBoxResult.OK)
                    {
                        CMessageBoxResult confirmToDel2 = CMessageBox.Show("您确定吗？", CMessageBoxButton.OKCancel);
                        if (confirmToDel2 == CMessageBoxResult.OK)
                        {
                            if (SelectedElement != null)
                            {
                                string result1 = SelectedElement.Name.ToString();//获得当前病历号
                                NameToDelete = result1;
                                NameToDelete1 = result1;
                                HMSDATA.Connect();
                                HMSDATA.SheetCon();
                                HMSDATA.DeleteRows(result1);
                                HMSDATA.CloseCon();
                                DetelteAll = true;
                                max_delete = dataGrid1.Items.Count;
                                delete = true;
                            }
                        }
                    }
                }
                else
                {
                    CMessageBoxResult confirmToDel = CMessageBox.Show("确定要删除该条记录吗？", CMessageBoxButton.OKCancel);
                    if (confirmToDel == CMessageBoxResult.OK)
                    {
                        SelectRowNum_Change = dataGrid.SelectedIndex;
                        if (SelectedElement != null && SelectedElement1 != null)
                        {
                            string result1 = SelectedElement.Name.ToString();//获得当前病历号
                            //string result = SelectedElement1.Num.ToString();//获得该病历号的次数
                            int a=Convert.ToInt32(SelectedElement1.Num)-1;
                            string result = Numtoshow.ElementAt(a);//获得该病历号的次数
                            NameToDelete = result1 + result + ".fot";
                            NameToDelete1 = result1;
                            DetelteAll = false;
                            HMSDATA.Connect();
                            HMSDATA.SheetCon();
                            HMSDATA.DeleteRow(result1 + result);
                            HMSDATA.CloseCon();
                            delete = true;
                        }
                    }
                }
                if (delete)
                {
                    //重新载入表格2
                    HMSDATA.Clear();
                    if (IsForfoot)
                    {
                        HMSDATA.SetPlace("TestData.accdb");
                        FileName = "TestData.accdb";
                    }
                    else
                    {
                        HMSDATA.SetPlace("TestData2.accdb");
                        FileName = "TestData2.accdb";
                    }
                    Saved = true;
                    LoadFile();
                    if (HMSDATA.IfCon == false)
                    {
                        CMessageBox.Show("连接失败！", CMessageBoxButton.OK);
                    }
                    DataGridView1.DataSource = HMSDATA.GetData().Tables[0];
                    for (int i = 0; i < 12; i++)
                    { DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable; DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; }
                    if (dataGrid_SelectedElement != null)
                    {
                        string result = dataGrid_SelectedElement.Name.ToString();
                        choosed = Convert.ToInt32(result);

                        Searchboard.Clear();
                        final[0] = null;//床号
                        final[1] = null;//性别
                        final[2] = dataGrid_SelectedElement.Name.ToString();//病历号
                        final[3] = dataGrid_SelectedElement.Age.ToString();//姓名
                        final[4] = null;//年龄
                        final[5] = null;//最大次数
                        final[6] = null;//住院日期
                        final[7] = null;//次数
                        final[8] = null;//备注
                        final[9] = null;//发布日期
                        final[10] = null;//修改日期
                        LoadGrid1();
                        fflushSearch(dpt.Search(final, Date, SearchOpiton));
                        SearchboardOpen = true;
                        DataGridView1.DataSource = Searchboard;
                        for (int i = 0; i < 12; i++)
                            DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
                        LoadGrid2();

                        try
                        {
                            DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(SelectRowNum_Change);
                            if (row == null)
                            {
                                dataGrid.UpdateLayout();
                                dataGrid.ScrollIntoView(dataGrid.Items[SelectRowNum_Change]);
                                row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(SelectRowNum_Change);
                                row.IsSelected = true;
                            }
                        }
                        catch
                        {
                        }
                        if (DetelteAll == false)
                        {
                            if (IsForfoot)
                            {
                                if (System.IO.File.Exists("footshow/" + NameToDelete))
                                {
                                    System.IO.File.Delete("footshow/" + NameToDelete);
                                }
                            }
                            else
                            {
                                if (System.IO.File.Exists("singleshow/" + NameToDelete))
                                {
                                    System.IO.File.Delete("singleshow/" + NameToDelete);
                                }
                            }

                        }
                        else
                        {
                            if (IsForfoot)
                            {
                                for (int i = 1; i < max_delete + 1; i++)
                                {
                                    if (System.IO.File.Exists("footshow/" + NameToDelete1 + i.ToString() + ".fot"))
                                    {
                                        System.IO.File.Delete("footshow/" + NameToDelete1 + i.ToString() + ".fot");
                                    }
                                }
                            }
                            else
                            {
                                for (int i = 1; i < max_delete + 1; i++)
                                {
                                    if (System.IO.File.Exists("singleshow/" + NameToDelete1 + i.ToString() + ".fot"))
                                    {
                                        System.IO.File.Delete("singleshow/" + NameToDelete1 + i.ToString() + ".fot");
                                    }
                                }
                            }

                        }
                    }
                }
            }
        }
        private void homeCtrlHToolStripMenuItem_Click()
        {
            HMSDATA.Copy(dpt.SyncDT());
            DataGridView1.DataSource = HMSDATA.GetData().Tables[0];
            SearchboardOpen = false;
            for (int i = 0; i < 12; i++)
                DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells;
        }
        private void UserControl_Loaded_2(object sender, RoutedEventArgs e)
        {
            if (PatientInfo.IsSave == true)
            {
                HMSDATA.Clear();
                if (IsForfoot == true)
                {
                    HMSDATA.SetPlace("TestData.accdb");
                    FileName = "TestData.accdb";
                }
                else
                {
                    HMSDATA.SetPlace("TestData2.accdb");
                    FileName = "TestData2.accdb";
                }
                Saved = true;
                LoadFile();
                homeCtrlHToolStripMenuItem_Click();
                if (HMSDATA.IfCon == false)
                {
                    CMessageBox.Show("连接失败！", CMessageBoxButton.OK);
                }
                HMSDATA.NewRow(PatientInfo.Add);
                HMSDATA.CloseCon();//操作完成后关闭连接
                //重新载入表格2
                HMSDATA.Clear();
                if (IsForfoot == true)
                {
                    HMSDATA.SetPlace("TestData.accdb");
                    FileName = "TestData.accdb";
                }
                else
                {
                    HMSDATA.SetPlace("TestData2.accdb");
                    FileName = "TestData2.accdb";
                }
                Saved = true;
                LoadFile();
                if (HMSDATA.IfCon == false)
                {
                    CMessageBox.Show("连接失败！", CMessageBoxButton.OK);
                }
                DataGridView1.DataSource = HMSDATA.GetData().Tables[0];
                for (int i = 0; i < 12; i++)
                { DataGridView1.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable; DataGridView1.Columns[i].AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells; }

                DataGridRow row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(PatientInfo.SelectRowNum);
                if (row != null)
                {
                    dataGrid.UpdateLayout();
                    dataGrid.ScrollIntoView(dataGrid.Items[PatientInfo.SelectRowNum]);
                    row = (DataGridRow)dataGrid.ItemContainerGenerator.ContainerFromIndex(PatientInfo.SelectRowNum);
                    row.IsSelected = true;
                }
                PatientInfo.IsSave = false;
                PatientInfo.PatientIdToSave = null;
                PatientInfo.NameToSave = null;
                PatientInfo.SelectRowNum = 0;
            }
            Loadnewornot();
            LoadGrid1();
            HMSDATA.CloseCon();
            try
            {
                this.dataGrid.SelectedIndex = 0;
            }
            catch
            {
                //Do nothing
            }
        }
    }
    public class Member
    {
        public string Name { get; set; }
        public string Age { get; set; }
    }//第一个数据显示类
    public class Member2
    {
        public string Num { get; set; }
    }//第二个数据显示类
}
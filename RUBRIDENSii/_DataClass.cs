using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using System.Data.OleDb;
using System.Data.SqlClient;
using ADOX;

namespace RUBRIDENSii
{
    #region 字段定义
    partial class _DataClass//字段定义
    {
        public bool IfCon = false;//与数据库连接是否成功
        private bool _IfSheet = false;//与数据库表格连接是否完成
        private string _DataPlace = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";//OLEDB连接数据库需要的参数 暂缺数据库文件位置
        private string _DataFile;//输入后接在_DataPlace后边
        private string _SheetName = "select * from ";//连接表格需要的参数 暂缺表格名字
        private OleDbDataAdapter _DataUpdate;//更新数据库需要的Adapter
        private OleDbConnection _DataCon;//与数据库的连接
        private DataSet _Data = new DataSet();//数据库的可修改缓存
        private int _MaxLine;//表格最大行数
        private string _Sheet;//表格真实名字
    };
    #endregion

    #region 方法定义
    partial class _DataClass//方法定义
    {
        public void New(string TableName)
        {
            ADOX.Catalog _Cat = new ADOX.Catalog();
            ADOX.Table _Table = new ADOX.Table();
            try
            {
                if (System.IO.File.Exists(_DataFile))
                {
                    System.IO.File.Delete(_DataFile);
                }
                _Cat.Create(_DataPlace + _DataFile);
                _Table.Name = TableName;
                _Table.ParentCatalog = _Cat;
                ADOX.Column col = new ADOX.Column();
                col.ParentCatalog = _Cat;
                col.Type = ADOX.DataTypeEnum.adInteger;
                col.Name = "id";
                col.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                //col.Properties["AutoIncrement"].Value = true;
                _Table.Columns.Append(col, ADOX.DataTypeEnum.adInteger, 0);
                _Table.Keys.Append("PrimaryKey", ADOX.KeyTypeEnum.adKeyPrimary, "id", "", "");
                _Cat.Tables.Append(_Table);

                ADOX.Column col2 = new ADOX.Column();
                col2.ParentCatalog = _Cat;
                col2.Name = "床号";
                col2.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col2, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col3 = new ADOX.Column();
                col3.ParentCatalog = _Cat;
                col3.Name = "性别";
                col3.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col3, ADOX.DataTypeEnum.adVarChar, 50);

                ADOX.Column col4 = new ADOX.Column();
                col4.ParentCatalog = _Cat;
                col4.Name = "病历号";
                col4.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col4, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col5 = new ADOX.Column();
                col5.ParentCatalog = _Cat;
                col5.Name = "姓名";
                col5.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col5, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col6 = new ADOX.Column();
                col6.ParentCatalog = _Cat;
                col6.Name = "年龄";
                col6.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col6, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column coladd = new ADOX.Column();
                coladd.ParentCatalog = _Cat;
                coladd.Name = "最大次数";
                coladd.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(coladd, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col7 = new ADOX.Column();
                col7.ParentCatalog = _Cat;
                col7.Name = "住院日期";
                col7.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col7, ADOX.DataTypeEnum.adInteger, 100);

                ADOX.Column col8 = new ADOX.Column();
                col8.ParentCatalog = _Cat;
                col8.Name = "次数";
                col8.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col8, ADOX.DataTypeEnum.adInteger, 100);

                ADOX.Column col9 = new ADOX.Column();
                col9.ParentCatalog = _Cat;
                col9.Name = "备注";
                col9.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col9, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col10 = new ADOX.Column();
                col10.ParentCatalog = _Cat;
                col10.Name = "发布日期";
                col10.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col10, ADOX.DataTypeEnum.adVarChar, 100);

                ADOX.Column col11 = new ADOX.Column();
                col11.ParentCatalog = _Cat;
                col11.Name = "修改日期";
                col11.Properties["Jet OLEDB:Allow Zero Length"].Value = false;
                _Table.Columns.Append(col11, ADOX.DataTypeEnum.adVarChar, 100);

                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_Table);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_Cat.ActiveConnection);
                System.Runtime.InteropServices.Marshal.FinalReleaseComObject(_Cat);
                _Table = null;
                _Cat = null;
                GC.WaitForPendingFinalizers();
                GC.Collect();
                return;
            }
            catch
            {
                return;
           }
        }//建立一个新的.accdb文件（需要名字）
        public void SetPlace(string Place)
        {
            _DataFile = Place;
            return;
        } //确定文件位置及名字
        public void SetSheetName(string Name)
        {
            _Sheet = Name;
            return;
        } //确定表格名字
        public bool Connect()
        {
            try
            {
                _DataCon = new OleDbConnection(_DataPlace + _DataFile);
                _DataCon.Open();
                IfCon = true;
            }
            catch
            {
                IfCon = false;
            }
            return IfCon;
        } //连接数据库（需要名字，位置信息被确定）
        public bool SheetCon()
        {
            try
            {
                _DataUpdate = new OleDbDataAdapter(_SheetName + _Sheet, _DataCon);
                _IfSheet = true;
            }
            catch
            {
                _IfSheet = false;
            }
            return _IfSheet;
        }//连接数据库表格（需要数据库已连接）
        public bool Fill_DataSet()
        {
            if (IfCon == false || _IfSheet == false)
                return false;
            try
            {
                _DataUpdate.Fill(_Data);
                _MaxLine = _Data.Tables[0].Rows.Count;
                return true;
            }
            catch
            {
                return false;
            }
        }//用数据库表格填满缓冲数据库_Data 
        public void Save()//更新并保存数据库至硬盘
        {
            try
            {
                //
                OleDbCommandBuilder CB = new OleDbCommandBuilder(_DataUpdate);
                _DataUpdate.InsertCommand = CB.GetInsertCommand();
                _DataUpdate.Update(_Data);
                _DataUpdate.InsertCommand = CB.GetInsertCommand();
                return;
            }
            catch
            {
                 System.Windows.MessageBox.Show("保存未成功…");
            }             
        }
        public DataSet GetData()
        {
            return _Data;
        }//读取_Data的数据 只读
        public void NewRow(string[] New)
        {
            int id = 1;
            int temp = 1;

            for (int i = 0; i < _MaxLine; i++ )
            {
                temp = Convert.ToInt32(_Data.Tables[0].Rows[i]["id"]) + 1;
                if (temp > id)
                    id = temp;
            }
            id = id + 1;
            StringBuilder Total = new StringBuilder();
            Total.Append("insert into HMSDATA(id,床号,性别,病历号,姓名,年龄,最大次数,住院日期,次数,备注,发布日期,修改日期)");
            Total.Append("values(" + id + ",'" + New[0] + "','" + New[1] + "','" + New[2] + "','" + New[3] + "','" + New[4] + "','" + New[5] + "','" + New[6] + "','" + New[7] + "','" + New[8] + "','" + New[9] + "','" + New[10] + "')");
            OleDbCommand comm = new OleDbCommand(Total.ToString(), _DataCon);
            if (comm.ExecuteNonQuery() > 0)
            {
//                CMessageBox.Show("添加纪录成功！", CMessageBoxButton.OK);
            }
            else
            {
//                CMessageBox.Show("添加纪录失败！", CMessageBoxButton.OK);
            }
            _MaxLine++;
            return;
        }//新建行并保存
        public void DeleteRows(string num)
        {//输入参数为病人的病历号和时间
            try
            {
                StringBuilder Total = new StringBuilder();
                Total.Append("delete from HMSDATA where 病历号=");
                Total.Append("'" + num + "'");
                OleDbCommand comm = new OleDbCommand(Total.ToString(), _DataCon);
                if (comm.ExecuteNonQuery() > 0)
                {
//                    CMessageBox.Show("删除纪录成功！", CMessageBoxButton.OK);
                }
                else
                {
//                    CMessageBox.Show("删除纪录失败！", CMessageBoxButton.OK);
                }
            }
            catch
            {

            }
            return;
        }//删除多行
        public void DeleteRow(string num)
        {//输入参数为病人的病历号和时间
            try
            {
                StringBuilder Total = new StringBuilder();
                Total.Append("delete from HMSDATA where 修改日期=");
                Total.Append("'" + num + "'");
                OleDbCommand comm = new OleDbCommand(Total.ToString(), _DataCon);
                if (comm.ExecuteNonQuery() > 0)
                {
//                    CMessageBox.Show("删除纪录成功！", CMessageBoxButton.OK);
                }
                else
                {
//                    CMessageBox.Show("删除纪录失败！", CMessageBoxButton.OK);
                }
            }
            catch
            {

            }
            return;
        }//删除行
        public void CloseCon()
        {
            if (IfCon == true)
            {
                _DataCon.Close();
                IfCon = false;
            }
        }//Close connection.
        public void Clear()
        {
            //_Data = new DataSet();
            //_DataFile = null;
            //_Sheet = null;
            //if (IfCon == true)
            //    _DataCon.Close();

//            _Data = new DataSet();
            _Data.Clear();
            _DataFile = null;
            _Sheet = null;
            if (IfCon == true)
            {
                _DataCon.Close();
                IfCon = false;
            }
            _IfSheet = false;//与数据库表格连接是否完成
            _DataPlace = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=";//OLEDB连接数据库需要的参数 暂缺数据库文件位置
            _SheetName = "select * from ";//连接表格需要的参数 暂缺表格名字
            _DataUpdate = null;//更新数据库需要的Adapter
            _DataCon = null;//与数据库的连接
            _MaxLine = 0;//表格最大行数
        }//清空整个数据类
        public void Copy(DataTable datatable)
        {
            _Data.Tables[0].Clear();
            int i = 0;
            while (i < datatable.Rows.Count)
            {
                //_Data.Tables[0].ImportRow(dataset.Tables[0].Rows[i]);
                {
                    DataRow NewR = _Data.Tables[0].NewRow();
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
                    _Data.Tables[0].Rows.Add(NewR);
                }
                i++;
            }
            _MaxLine = datatable.Rows.Count;
            return;
        }//刷新datatable 用于将新的datatable完全刷新当前表
        public string GetPlace()
        {
            return _DataFile;
        }//返回文件名 包含路径
    };
    #endregion
}

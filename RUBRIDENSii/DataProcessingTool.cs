using System;
using System.Collections;
using System.Data;
using System.Text.RegularExpressions;
using Merchandise;

namespace DataProcessingTool
{
    //数据处理工具类
    public class DPT
    {
        #region 字段定义

        //mdseAL为存放商品子类的ArrayList(AL)
        private ArrayList mdseAL = new ArrayList();
        //mdseDT为存放商品子类的DataTable(DT)，此DT与窗口中所显示的表格同步
        private DataTable mdseDT;
        //maxIndex为mdseAL中商品index的最大值
        private int maxIndex = 0;

        #endregion

        #region 方法定义

        #region 构造相关

        //默认构造函数
        public DPT()
        {
        }

        //将mdseDT置为空DT的构造函数
        public DPT(DataTable blankDT)
        {
            mdseDT = blankDT;
        }

        #endregion

        #region 基础操作相关

        //复制DataRow
        private void CopyRow(DataRow from, ref DataRow to)
        {
            to["id"] = Convert.ToInt32(from["id"].ToString());
            to["床号"] = from["床号"].ToString();
            to["性别"] = from["性别"].ToString();
            to["病历号"] = from["病历号"].ToString();
            to["姓名"] = from["姓名"].ToString();
            to["年龄"] = from["年龄"].ToString();
            to["最大次数"] = from["最大次数"].ToString();
            to["住院日期"] = from["住院日期"].ToString();
            to["次数"] = from["次数"].ToString();
            to["发布日期"] = from["发布日期"].ToString();
            to["修改日期"] = from["修改日期"].ToString();
            to["备注"] = from["备注"].ToString();
        }

        //初始化操作，维护三个私有字段
        public void InitializeAL(DataSet source)
        {
            foreach (DataRow row in source.Tables[0].Rows)
            {
                if (maxIndex < Convert.ToInt32(row["id"].ToString()))
                    maxIndex = Convert.ToInt32(row["id"].ToString());
                mdseAL.Add(new putong(row));

            }
            mdseDT = SyncDT();
        }

        //修改操作，维护三个私有字段
        public DataTable ChangeAL(DataRow source)
        {
            //处理mdseAL
            foreach (Mdse mdse in mdseAL)
            {
                //如果找到了mdseAL中的对应商品，对其修改
                if (mdse.index == source["id"].ToString())
                {
                    ((putong)mdse).ChangeEE(source);
                }
            }
            //处理mdseDT
            foreach (DataRow row in mdseDT.Rows)
            {
                //如果找到了mdseDT中的对应商品，对其修改
                if (row["id"].ToString() == source["id"].ToString())
                {
                    row["床号"] = source["床号"].ToString();
                    row["性别"] = source["性别"].ToString();
                    row["病历号"] = source["病历号"].ToString();
                    row["姓名"] = source["姓名"].ToString();
                    row["年龄"] = source["年龄"].ToString();
                    row["最大次数"] = source["最大次数"].ToString();
                    row["住院日期"] = source["住院日期"].ToString();
                    row["次数"] = source["次数"].ToString();
                    row["发布日期"] = source["发布日期"].ToString();
                    row["修改日期"] = source["修改日期"].ToString();
                    row["备注"] = source["备注"].ToString();
                    break;
                }
            }
            return mdseDT;
        }

        //删除操作，维护三个私有字段
        public DataTable DeleteAL(DataRow source)
        {
            //处理mdseAL
            foreach (Mdse mdse in mdseAL)
            {
                //如果找到了mdseAL中的对应商品，对其删除
                if (mdse.index == source["id"].ToString())
                {
                    mdseAL.Remove((putong)mdse);
                    break;
                }
            }
            //处理mdseDT
            foreach (DataRow row in mdseDT.Rows)
            {
                //如果找到了mdseDT中的对应商品，对其删除
                if (row["id"].ToString() == source["id"].ToString())
                {
                    row.Delete();
                    mdseDT.AcceptChanges();
                    break;
                }
            }
            return mdseDT;
        }

        //添加操作，维护三个私有字段
        public DataTable AddAL(DataRow source)
        {
            DataRow newSource = source;

            //以当前系统时间作为该记录的发布、修改日期
            newSource["发布日期"] = DateTime.Now.ToString();
            newSource["修改日期"] = DateTime.Now.ToString();

            //以maxIndex + 1作为新的index，确保index不重复
            newSource["id"] = ++maxIndex;
            //处理mdseAL
            mdseAL.Add(new putong(newSource));
            //处理mdseDT
            DataRow newRow = mdseDT.NewRow();
            CopyRow(newSource, ref newRow);
            mdseDT.Rows.Add(newRow);
            return mdseDT;
        }

        //将mdseAL中的数据同步至DT
        public DataTable SyncDT()
        {
            mdseDT.Clear();
            foreach (Mdse mdse in mdseAL)
            {
                DataRow newRow = mdseDT.NewRow();
                newRow["id"] = Convert.ToInt32(mdse.index);
                newRow["床号"] = mdse.firstCl;
                newRow["性别"] = mdse.secondCl;
                newRow["病历号"] = mdse.LongThirdCl();
                newRow["姓名"] = mdse.name;
                newRow["年龄"] = mdse.state;
                newRow["最大次数"] = mdse.thReason;
                newRow["住院日期"] = mdse.buyPr;
                newRow["次数"] = mdse.sellPr;
                newRow["发布日期"] = mdse.publishDT;
                newRow["修改日期"] = mdse.updateDT;
                newRow["备注"] = ((putong)mdse).remarkEE;
                mdseDT.Rows.Add(newRow);
            }
            return mdseDT;
        }

        //返回完整DT，但不更新mdseDT
        public DataTable FullDataTable()
        {
            DataTable fdt = mdseDT.Clone();
            foreach (Mdse mdse in mdseAL)
            {
                DataRow newRow = fdt.NewRow();
                newRow["id"] = Convert.ToInt32(mdse.index);
                newRow["床号"] = mdse.firstCl;
                newRow["性别"] = mdse.secondCl;
                newRow["病历号"] = mdse.LongThirdCl();
                newRow["姓名"] = mdse.name;
                newRow["年龄"] = mdse.state;
                newRow["最大次数"] = mdse.thReason;
                newRow["住院日期"] = mdse.buyPr;
                newRow["次数"] = mdse.sellPr;
                newRow["发布日期"] = mdse.publishDT;
                newRow["修改日期"] = mdse.updateDT;
                newRow["备注"] = ((putong)mdse).remarkEE;
                fdt.Rows.Add(newRow);
            }
            return fdt;
        }

        #endregion

        #region 搜索相关

        //返回备注
        private string Remark(Mdse mdse)
        {
            return ((putong)mdse).remarkEE;
        }
        //搜索函数，返回字符串是否匹配
        private bool Matched(string item, string str, int mode)
        {
            try
            {
                switch (mode)
                {
                    case 0://全匹配
                        return item == str;
                    case 1://str取消转义，仅作为子串来匹配
                        return Regex.IsMatch(item, Regex.Escape(str));
                    case 2://str作为正则表达式来匹配
                        return Regex.IsMatch(item, str);
                    default:
                        throw (new System.ArgumentOutOfRangeException("search mode"));
                }
            }
            catch
            {
                return false;
            }
        }
        //搜索函数，返回由符合条件的DT
        public DataTable Search(string[] str, DateTime[] dt, int[] mode)
        {
            DateTime blank = new DateTime(1, 1, 1, 0, 0, 0);
            mdseDT.Clear();
            foreach (Mdse mdse in mdseAL)
            {
                //如果满足搜索的各种条件，就把这条数据放进mdseDT里
                if (str[2] == "" || Matched(mdse.thirdCl, str[2], mode[0]))
                    if (str[3] == "" || Matched(mdse.name, str[3], mode[0]))
                    {
                        DataRow row = mdseDT.NewRow();
                        row["id"] = Convert.ToInt32(mdse.index);
                        row["床号"] = mdse.firstCl;
                        row["性别"] = mdse.secondCl;
                        row["病历号"] = mdse.LongThirdCl();
                        row["姓名"] = mdse.name;
                        row["年龄"] = mdse.state;
                        row["最大次数"] = mdse.thReason;
                        row["住院日期"] = mdse.buyPr;
                        row["次数"] = mdse.sellPr;
                        row["发布日期"] = mdse.publishDT;
                        row["修改日期"] = mdse.updateDT;
                        row["备注"] = Remark(mdse);
                        mdseDT.Rows.Add(row);
                    }
            }
            return mdseDT;
        }

        #endregion

        #endregion
    }
}

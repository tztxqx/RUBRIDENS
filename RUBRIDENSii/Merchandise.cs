using System;
using System.Data;

namespace Merchandise
{
    //用户类
    public class Mdse
    {
        #region 字段定义
        //含义见属性定义
        private string _index;
        private string _firstCl;//床号
        private string _secondCl;//性别
        private string _thirdCl;
        private string _name;
        private string _state;//年龄
        private string _thReason;//
        private string _buyPr;
        private string _sellPr;
        private string _publishDT;
        private string _updateDT;
        #endregion

        #region 属性定义
        public string index            //id
        {
            get
            {
                return _index;
            }
            protected set
            {
                _index = value;
            }
        }
        public string firstCl          //床号
        {
            get
            {
                return _firstCl;
            }
            protected set
            {
                _firstCl = value;
            }
        }
        public string secondCl         //性别
        {
            get
            {
                return _secondCl;
            }
            protected set
            {
                _secondCl = value;
            }
        }
        public string thirdCl          //病历号
        {
            get
            {
                return _thirdCl;
            }
            protected set
            {
                _thirdCl = value;
            }
        }
        public string name             //姓名
        {
            get
            {
                return _name;
            }
            protected set
            {
                _name = value;
            }
        }
        public string state            //年龄
        {
            get
            {
                return _state;
            }
            protected set
            {
                _state = value;
            }
        }
        public string thReason         //最大次数
        {
            get
            {
                return _thReason;
            }
            protected set
            {
                _thReason = value;
            }
        }
        public string buyPr            //住院日期
        {
            get
            {
                return _buyPr;
            }
            protected set
            {
                _buyPr = value;
            }
        }
        public string sellPr           //次数
        {
            get
            {
                return _sellPr;
            }
            protected set
            {
                _sellPr = value;
            }
        }
        public string publishDT        //发布日期
        {
            get
            {
                return _publishDT;
            }
            protected set
            {
                _publishDT = value;
            }
        }
        public string updateDT         //修改日期
        {
            get
            {
                return _updateDT;
            }
            protected set
            {
                _updateDT = value;
            }
        }
        #endregion

        #region 方法定义

        //构造函数
        public Mdse()
        {
        }

        //通过source中的内容对商品初始化
        public Mdse(DataRow source)
        {
            index = source["id"].ToString();
            firstCl = source["床号"].ToString();
            secondCl = source["性别"].ToString();
            thirdCl = ShortThirdCl(source["病历号"].ToString());
            name = source["姓名"].ToString();
            state = source["年龄"].ToString();
            thReason = source["最大次数"].ToString();
            buyPr = source["住院日期"].ToString();
            sellPr = source["次数"].ToString();
            publishDT = source["发布日期"].ToString();
            updateDT = source["修改日期"].ToString();
        }

        //通过source中的内容对商品修改，更新修改时间
        public void ChangeMdse(DataRow source)
        {
            index = source["id"].ToString();
            firstCl = source["床号"].ToString();
            secondCl = source["性别"].ToString();
            thirdCl = ShortThirdCl(source["病历号"].ToString());
            name = source["姓名"].ToString();
            state = source["年龄"].ToString();
            thReason = source["最大次数"].ToString();
            buyPr = source["住院日期"].ToString();
            sellPr = source["次数"].ToString();
            publishDT = source["发布日期"].ToString();
            updateDT = DateTime.Now.ToString();
        }

        #region 字符串处理函数

        //把文字版thirdCl转成数字版thirdCl
        public string ShortThirdCl(string t)
        {
            return t;
        }

        //把数字版thirdCl转成文字版thirdCl
        public string LongThirdCl()
        {
            return thirdCl;
        }

        #endregion

        #endregion
    }

    //普通用户类
    public class putong : Mdse
    {
        #region 字段定义
        private string _remarkEE;
        #endregion

        #region 属性定义
        public string remarkEE      //电器备注，保修期
        {
            get
            {
                return _remarkEE;
            }
            protected set
            {
                _remarkEE = value;
            }
        }
        #endregion

        #region 方法定义

        //构造函数
        public putong()
        {
        }

        //通过source中的内容对电器初始化
        public putong(DataRow source)
            : base(source)
        {
            remarkEE = source["备注"].ToString();
        }

        //通过source中的内容对电器修改
        public void ChangeEE(DataRow source)
        {
            ChangeMdse(source);
            remarkEE = source["备注"].ToString();
        }

        #endregion
    }
}
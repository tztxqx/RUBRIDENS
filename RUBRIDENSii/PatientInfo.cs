using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RUBRIDENSii
{
    static class PatientInfo
    {
        public static String PatientData;
        //登陆信息
        public static String PatientName;
        public static String PatientAge;
        public static String PatientId;
        static public String PatientBed;
        static public String PatientSex;
        //状态选择
        static public bool loadornot = false;
        static public bool newornot = false;
        static public bool readyornot = true;


        //测量完成的时间yyyy-MM-dd HH:mm:ss
        static public string finishtime;


        //
        static public bool IsSave = false;
        public static String PatientIdToSave = null;
        public static String NameToSave = null;
        public static int SelectRowNum = 0;
        static public bool IsAdd = false;//判断当前是否为添加测量的跳转

        //
        public static int timelimit;

        //
        public static bool IsSavefailed = false;
        public static bool IsAddAndSavefailed = false;

        //模式选择问题
        static public bool IsForfoot = true;

        public static string[] Add = new string[11];
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatteryPower.Helpers
{
    class Param
    {
        // public static string APPFILEPATH = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\BatteryPower\";
        // 启动目录
        public static string APPFILEPATH = System.Windows.Forms.Application.StartupPath;
        // 蓄电池数据
        public static string BATTERY_FILE = APPFILEPATH + @"\data\batterys.xml";
        // 端口数据
        public static string PORT_FILE = APPFILEPATH + @"\data\port.xml";
        // 电压数据
        public static string VOLTAGE_FILE = APPFILEPATH + @"\data\voltage.csv";
        // 最新电压数据：采集时间、地址、第1节、第2节、...第24节
        public static List<object[]> CURRENT_VOLTAGE_DATA = new List<object[]>();

        //static Param()
        //{
        //    var data = new object[] { "2019-11-03 18:43:22", "70", 2.801, 2.802, 2.803, 2.804, 2.805, 2.806, 2.807, 2.808, 2.809, 2.810, 2.811, 2.812, 2.813, 2.814, 2.815, 2.816, 2.817, 2.818, 2.819, 2.820, 2.821, 2.822, 2.823, 2.824 };
        //    CURRENT_VOLTAGE_DATA.Add(data);
        //}
    }
}

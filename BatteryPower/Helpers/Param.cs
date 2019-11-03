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
    }
}

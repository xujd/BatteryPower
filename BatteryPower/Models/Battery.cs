using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatteryPower.Models
{
    public class Battery
    {
        public int uid { get; set; }
        public string id { get; set; }
        public string address { get; set; }
        public string oprUser { get; set; }
        public string keyTrainNo { get; set; }
        public string batteryType { get; set; }
        public double threshold { get; set; }
        public double coefficient { get; set; }  // 电压参考系数
        public int collectCycle { get; set; }
        public string isEnabled { get; set; }
        public string lastModifyTime { get; set; }

        public Battery()
        {
            this.collectCycle = -1;
            this.isEnabled = "否";
        }
    }
}

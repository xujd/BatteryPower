using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatteryPower.Models
{
    public class PortConfig
    {
        public string serialName { get; set; }
        public string baudRate { get; set; }
        public string dataBit { get; set; }
        public string stopBit { get; set; }
        public string parityBit { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BatteryPower.Helpers
{
    class MathHelper
    {
        public static int MaximumCommonDivisor(int a, int b)//最大公约数 
        {
            if (a < b) { a = a + b; b = a - b; a = a - b; }
            return (a % b == 0) ? b : MaximumCommonDivisor(a % b, b);
        }

        public static int LeastCommonMultiple(int a, int b)//最小公倍数 
        {
            return a * b / MaximumCommonDivisor(a, b);
        }

        public static int LeastCommonMultiple(List<int> list)//最小公倍数 
        {
            if (list.Count == 0)
            {
                return 1;
            }
            var result = list[0];
            for (var i = 1; i < list.Count; i++)
            {
                result = LeastCommonMultiple(result, list[i]);
            }

            return result;
        }
    }
}

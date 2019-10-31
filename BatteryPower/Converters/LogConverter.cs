using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Data;

namespace BatteryPower.Converters
{
    class LogConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (parameter.ToString() == "A")
            {
                return value.ToString().Substring(0, value.ToString().IndexOf(" "));
            }
            else
            {
                return value.ToString().Substring(value.ToString().IndexOf(" ") + 1);
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}

using BatteryPower.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatteryPower.Views
{
    /// <summary>
    /// Interaction logic for CollectLogView.xaml
    /// </summary>
    public partial class CollectLogView : UserControl
    {
        public CollectLogView()
        {
            InitializeComponent();

            this.lbLog.ItemsSource = LogHelper.LogList;
        }
    }
}

using BatteryPower.Helpers;
using BatteryPower.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BatteryPower
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private string sysName = "蓄电池管理系统";
        public MainWindow()
        {
            InitializeComponent();
            LogHelper.dispatcher = this.Dispatcher;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as System.Windows.Controls.MenuItem).Tag.ToString();
            switch (tag)
            {
                case "about":
                    (new About()).ShowDialog();
                    break;
                case "debug":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new DebugView());
                    this.Title = this.sysName + "-巡检调试";
                    break;
                case "resource":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new ResourceView());
                    this.Title = this.sysName + "-资源管理";
                    break;
                case "collect":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new CollectView());
                    this.Title = this.sysName + "-采集管理";
                    break;
                case "indicator":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new IndicatorView());
                    this.Title = this.sysName + "-指标管理";
                    break;
                case "report":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new ReportView());
                    this.Title = this.sysName + "-报表管理";
                    break;
                case "warn":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new WarnView());
                    this.Title = this.sysName + "-预警管理";
                    break;
                case "show":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new ShowView());
                    this.Title = this.sysName + "-展示管理";
                    break;

            }
        }
    }
}

using BatteryPower.Helpers;
using BatteryPower.Models;
using BatteryPower.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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
        private string pubTime = "2019-11-01";
        public MainWindow()
        {
            InitializeComponent();
            var version = Assembly.GetExecutingAssembly().GetName().Version;
            pubTime = System.IO.File.GetLastWriteTime(this.GetType().Assembly.Location).ToString();

            if (DateTime.Now > DateTime.Parse(pubTime).AddDays(400))
            {
                LogHelper.WriteLog(LogType.ERROR, "软件授权已到期！请联系管理人员。");
            }
            LogHelper.dispatcher = this.Dispatcher;

            this.Closing += MainWindow_Closing;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var tag = (sender as System.Windows.Controls.MenuItem).Tag.ToString();
            if(this.conentGrid.Children.Count>0 && this.conentGrid.Children[0] is ShowView)
            {
                (this.conentGrid.Children[0] as ShowView).Stop();
            }
            switch (tag)
            {
                case "home":
                    this.conentGrid.Children.Clear();
                    this.homeView.Visibility = Visibility.Visible;
                    this.Title = this.sysName + "-首页";
                    break;
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
                    this.Title = this.sysName + "-采集任务配置";
                    break;
                case "log":
                    this.conentGrid.Children.Clear();
                    this.conentGrid.Children.Add(new CollectLogView());
                    this.Title = this.sysName + "-采集日志查看";
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
            if(tag != "home" && tag != "about")
            {
                this.homeView.Visibility = Visibility.Collapsed;
                this.conentGrid.Visibility = Visibility.Visible;
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            this.homeView.StopTask();
            if (this.conentGrid.Children.Count > 0 && this.conentGrid.Children[0] is ShowView)
            {
                (this.conentGrid.Children[0] as ShowView).Stop();
            }
            LogHelper.Stop();
        }
    }
}

using BatteryPower.Helpers;
using BatteryPower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
    /// Interaction logic for CollectView.xaml
    /// </summary>
    public partial class CollectView : UserControl
    {
        private string dataFile
        {
            get { return Param.BATTERY_FILE; }
        }
        private ObservableCollection<Battery> batteryList = new ObservableCollection<Battery>();
        private Battery battery = null;
        private Boolean isDoing = false;

        public CollectView()
        {
            InitializeComponent();

            var list = XmlHelper.LoadFromXml(this.dataFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            if (list != null)
            {
                this.batteryList = list;
            }

            dataGrid.ItemsSource = this.batteryList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            int cycle = -1;
            if (!int.TryParse(tbCollectCycle.Text, out cycle))
            {
                MessageBox.Show("采集周期为非法数字！");
                return;
            }
            if (cycle < 5)
            {
                MessageBox.Show("采集周期最小为5分钟！");
                return;
            }

            this.battery.collectCycle = cycle;
            this.battery.isEnabled = this.rbEnabled.IsChecked.HasValue && this.rbEnabled.IsChecked.Value ? "是" : "否";
            this.battery.lastModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            this.batteryList.RemoveAt(this.batteryList.IndexOf(this.battery));
            this.batteryList.Add(this.battery);

            XmlHelper.SaveToXml(this.dataFile, this.batteryList);
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as Battery;
                this.tbId.Text = item.id;
                this.tbAddress.Text = item.address;
                this.tbCollectCycle.Text = item.collectCycle.ToString();
                this.rbEnabled.IsChecked = item.isEnabled == "是";
                this.rbDisabled.IsChecked = item.isEnabled == "否";

                this.battery = item;
            }
        }

        private void btnDo_Click(object sender, RoutedEventArgs e)
        {
            this.isDoing = !this.isDoing;

            // this.btnDo.Content = this.isDoing ? "停止执行" : "开始执行";
        }
    }
}

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
    /// Interaction logic for ResourceView.xaml
    /// </summary>
    public partial class ResourceView : UserControl
    {
        private ObservableCollection<Battery> dataList = new ObservableCollection<Battery>();
        private Battery battery = null;

        private string dataFile
        {
            get { return Param.BATTERY_FILE; }
        }

        public ResourceView()
        {
            InitializeComponent();

            var list = XmlHelper.LoadFromXml(this.dataFile, dataList.GetType()) as ObservableCollection<Battery>;
            if (list != null)
            {
                this.dataList = list;
            }
            this.dataGrid.ItemsSource = this.dataList;
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            double threshold = -1;
            if (!double.TryParse(tbThreshold.Text, out threshold))
            {
                MessageBox.Show("预警门限为非法数字！");
                return;
            }
            double coefficient = -1;
            if (!double.TryParse(tbCoefficient.Text, out coefficient))
            {
                MessageBox.Show("电压参考系数为非法数字！");
                return;
            }
            if (this.battery != null)
            {
                var tempList = this.dataList.Where(item => item.id == this.tbId.Text && this.battery.uid != item.uid);
                if (tempList.Count() > 0)
                {
                    MessageBox.Show("蓄电池编号重复！");
                    return;
                }
                tempList = this.dataList.Where(item => item.address == this.tbAddress.Text && this.battery.uid != item.uid);
                if (tempList.Count() > 0)
                {
                    MessageBox.Show("蓄电池地址重复！");
                    return;
                }
            }
            else
            {
                var tempList = this.dataList.Where(item => item.id == this.tbId.Text);
                if (tempList.Count() > 0)
                {
                    MessageBox.Show("蓄电池编号重复！");
                    return;
                }
                tempList = this.dataList.Where(item => item.address == this.tbAddress.Text);
                if (tempList.Count() > 0)
                {
                    MessageBox.Show("蓄电池地址重复！");
                    return;
                }
            }

            if (!string.IsNullOrEmpty(this.tbId.Text) && !string.IsNullOrEmpty(this.tbAddress.Text)
                //&& !string.IsNullOrEmpty(this.tbOprUser.Text) && !string.IsNullOrEmpty(this.tbKeyTrainNo.Text)
                //&& !string.IsNullOrEmpty(this.tbBatteryType.Text)
                )
            {
                if (this.battery != null)
                {
                    for (var i = 0; i < dataList.Count; i++)
                    {
                        if (dataList[i].uid == this.battery.uid) // 修改，先删除
                        {
                            dataList.RemoveAt(i);
                            break;
                        }
                    }
                }
                var data = new Battery();
                data.uid = this.dataList.Count == 0 ? 1 : (this.dataList.OrderByDescending(item => item.uid).First().uid + 1);
                data.id = this.tbId.Text;
                data.address = this.tbAddress.Text;
                data.oprUser = this.tbOprUser.Text;
                data.keyTrainNo = this.tbKeyTrainNo.Text;
                data.batteryType = this.tbBatteryType.Text;
                data.threshold = threshold;
                data.coefficient = coefficient;
                data.lastModifyTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");

                this.dataList.Add(data);

                this.battery = null;

                this.tbId.Text = "";
                this.tbAddress.Text = "";
                this.tbOprUser.Text = "";
                this.tbKeyTrainNo.Text = "";
                this.tbBatteryType.Text = "";
                this.tbThreshold.Text = "";
                this.tbCoefficient.Text = "";

                XmlHelper.SaveToXml(this.dataFile, this.dataList);
            }
            else
            {
                MessageBox.Show("属性格式错误，存在为空的项！");
            }
        }

        private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (e.AddedItems.Count > 0)
            {
                var item = e.AddedItems[0] as Battery;
                this.tbId.Text = item.id;
                this.tbAddress.Text = item.address;
                this.tbOprUser.Text = item.oprUser;
                this.tbKeyTrainNo.Text = item.keyTrainNo;
                this.tbBatteryType.Text = item.batteryType;
                this.tbThreshold.Text = item.threshold.ToString();
                this.tbCoefficient.Text = item.coefficient>0?item.coefficient.ToString():"";

                this.battery = item;
            }
        }

        private void btnDelete_Click(object sender, RoutedEventArgs e)
        {
            if (this.dataGrid.SelectedItem == null)
            {
                MessageBox.Show("未选中任何记录！");
                return;
            }
            this.battery = null;

            this.tbId.Text = "";
            this.tbAddress.Text = "";
            this.tbOprUser.Text = "";
            this.tbKeyTrainNo.Text = "";
            this.tbBatteryType.Text = "";
            this.tbThreshold.Text = "";
            this.tbCoefficient.Text = "";

            this.dataList.Remove(this.dataGrid.SelectedItem as Battery);
            XmlHelper.SaveToXml(this.dataFile, this.dataList);
        }

        private void btnReset_Click(object sender, RoutedEventArgs e)
        {
            this.battery = null;
            this.tbId.Text = this.tbAddress.Text = this.tbOprUser.Text =
            this.tbKeyTrainNo.Text = this.tbBatteryType.Text = this.tbThreshold.Text = "";
            this.tbCoefficient.Text = "";
        }
    }
}

using BatteryPower.Comps;
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
using System.Windows.Threading;

namespace BatteryPower.Views
{
    /// <summary>
    /// Interaction logic for ShowView.xaml
    /// </summary>
    public partial class ShowView : UserControl
    {
        private string batteryFile
        {
            get { return Param.BATTERY_FILE; }
        }
        private List<Battery> batteryList = new List<Battery>();

        private SolidColorBrush greenBrush = new SolidColorBrush(Color.FromRgb(0, 231, 0));
        private SolidColorBrush redBrush = new SolidColorBrush(Colors.Red);
        private SolidColorBrush grayBrush = new SolidColorBrush(Colors.LightGray);

        private List<FuncTitle> funcTitleList = new List<FuncTitle>();
        private List<Label> valueLabelList = new List<Label>();
        private List<TextBlock> nameTextList = new List<TextBlock>();

        private DispatcherTimer timer = new DispatcherTimer();

        private double threshold = 0;

        private string sortOrder = "ASC";

        public ShowView()
        {
            InitializeComponent();

            var list = XmlHelper.LoadFromXml(this.batteryFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            if (list != null)
            {
                this.batteryList = list.Where(i => i.isEnabled == "是").OrderBy(i => i.uid).ToList();
            }

            if (this.batteryList.Count > 0)
            {
                this.threshold = this.batteryList.ElementAt(0).threshold;
                this.tbThreshold.Text = this.threshold.ToString();
            }

            // 创建视图元素
            this.CreateView();
            // 刷新界面
            this.RefreshView();

            timer.Interval = TimeSpan.FromSeconds(5);
            timer.Tick += Timer_Tick;
            timer.Start();
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            this.RefreshView();
        }

        private void RefreshView_old()
        {
            foreach (var item in this.funcTitleList)
            {
                object[] data = null;
                bool flag = false;
                for (var i = 0; i < Param.CURRENT_VOLTAGE_DATA.Count; i++)
                {
                    data = Param.CURRENT_VOLTAGE_DATA[i];
                    if (item.Tag.ToString() == data[1].ToString())
                    {
                        flag = true;
                        break;
                    }
                }
                if (flag)
                {
                    item.Title = string.Format("蓄电池地址：{0}（时间：{1}）", data[1], data[0]);
                    var labelList = this.valueLabelList.Where(i => i.Tag.ToString().StartsWith(data[1].ToString()));
                    for (var i = 0; i < labelList.Count(); i++)
                    {
                        labelList.ElementAt(i).Content = ((double)data[i + 2]).ToString("F3") + "V";
                        var threshold = this.threshold;
                        labelList.ElementAt(i).Background = (double)data[i + 2] < threshold ? redBrush : greenBrush;
                    }
                }
            }
        }

        private void RefreshView()
        {
            if (Param.CURRENT_VOLTAGE_DATA.Count > 0)
            {
                this.tbTime.Text = Param.CURRENT_VOLTAGE_DATA[0][0].ToString();
            }
            var dataList = new List<VoltageData>();
            var index = 0;
            foreach (var battery in this.batteryList)
            {
                bool flag = false;
                foreach (var item in Param.CURRENT_VOLTAGE_DATA)
                {
                    if (item[1].ToString() == battery.address)
                    {
                        for (var i = 2; i < item.Length; i++)
                        {
                            dataList.Add(new VoltageData() { Address = item[1].ToString(), No = ++index, Voltage = (double)item[i] });
                        }
                        flag = true;
                        break;
                    }
                }
                if (!flag)
                {
                    for (var i = 0; i < 24; i++)
                    {
                        dataList.Add(new VoltageData() { Address = battery.address, No = ++index, Voltage = this.sortOrder == "ASC" ? 250 : 0 });
                    }
                }
            }


            if (this.sortOrder == "ASC")
            {
                dataList = (from item in dataList orderby item.Voltage ascending select item).ToList();
            }
            else if (this.sortOrder == "DESC")
            {
                dataList = (from item in dataList orderby item.Voltage descending select item).ToList();
            }
            else
            {
                dataList = (from item in dataList orderby item.No ascending select item).ToList();
            }

            for (var i = 0; i < this.valueLabelList.Count; i++)
            {
                if (i < dataList.Count)
                {
                    this.nameTextList[i].Text = string.Format("第{0}节", dataList[i].No);
                    this.valueLabelList[i].Content = dataList[i].Voltage > 0 && dataList[i].Voltage < 250 ? dataList[i].Voltage.ToString("F3") : "-";
                    this.valueLabelList[i].Background = (dataList[i].Voltage == 0 || dataList[i].Voltage == 250) ? grayBrush : (dataList[i].Voltage < threshold ? redBrush : greenBrush);
                }
                else
                {
                    this.nameTextList[i].Text = "-";
                    this.valueLabelList[i].Content = "-";
                    this.valueLabelList[i].Background = grayBrush;
                }
            }
        }

        private void CreateView_old()
        {
            viewGrid.Children.Clear();
            viewGrid.RowDefinitions.Clear();

            for (var i = 0; i < this.batteryList.Count; i++)
            {
                var item = this.batteryList[i];
                viewGrid.RowDefinitions.Add(new RowDefinition() { Height = GridLength.Auto });
                viewGrid.RowDefinitions.Add(new RowDefinition());
                var funcTitle = new FuncTitle() { Tag = item.address, Title = string.Format("蓄电池地址：{0}（时间：{1}）", item.address, "-") };
                this.funcTitleList.Add(funcTitle);
                Grid.SetRow(funcTitle, i * 2);
                viewGrid.Children.Add(funcTitle);
                var grid = new Grid() { HorizontalAlignment = HorizontalAlignment.Stretch };
                Grid.SetRow(grid, i * 2 + 1);
                viewGrid.Children.Add(grid);
                var wrapPanel = new WrapPanel();
                grid.Children.Add(wrapPanel);
                for (var j = 0; j < 24; j++) // 24节电池
                {
                    var sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(11, 0, 0, 5) };
                    wrapPanel.Children.Add(sp);
                    var textBlock = new TextBlock() { Tag = item.address, Text = string.Format("第{0}节：", i * 24 + j + 1), Width = 54, VerticalAlignment = VerticalAlignment.Center };
                    sp.Children.Add(textBlock);
                    var label = new Label() { Tag = item.address + "-" + j, Content = "-V", Width = 65, Height = 23, Background = greenBrush, VerticalContentAlignment = VerticalAlignment.Center };
                    sp.Children.Add(label);
                    this.valueLabelList.Add(label);
                }
            }
        }

        private void CreateView()
        {
            wrapPanel.Children.Clear();

            for (var i = 0; i < this.batteryList.Count; i++)
            {
                var item = this.batteryList[i];
                for (var j = 0; j < 24; j++) // 24节电池
                {
                    var sp = new StackPanel() { Orientation = Orientation.Horizontal, Margin = new Thickness(11, 0, 0, 5) };
                    wrapPanel.Children.Add(sp);
                    var textBlock = new TextBlock() { Tag = item.address + "-" + j, Text = string.Format("第{0}节：", i * 24 + j + 1), Width = 54, VerticalAlignment = VerticalAlignment.Center };
                    sp.Children.Add(textBlock);
                    this.nameTextList.Add(textBlock);
                    var label = new Label() { Tag = item.address + "-" + j, Content = "-", Width = 65, Height = 30, Background = greenBrush, VerticalContentAlignment = VerticalAlignment.Center, HorizontalContentAlignment = HorizontalAlignment.Center };
                    sp.Children.Add(label);
                    this.valueLabelList.Add(label);
                }
            }
        }

        public void Stop()
        {
            this.timer.Stop();
            this.timer.Tick -= Timer_Tick;
        }

        private void tbThreshold_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                double data = -1;
                if (!double.TryParse(tbThreshold.Text, out data))
                {
                    MessageBox.Show("预警门限为非法数字！");
                    return;
                }
                this.threshold = data;
                // 刷新页面
                this.RefreshView();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.sortOrder = (sender as Button).Tag.ToString();
            if (this.sortOrder == "ASC")
            {
                this.imgAsc.Visibility = Visibility.Visible;
                this.imgDesc.Visibility = Visibility.Collapsed;
            }
            else if (this.sortOrder == "DESC")
            {
                this.imgAsc.Visibility = Visibility.Collapsed;
                this.imgDesc.Visibility = Visibility.Visible;
            }
            else
            {
                this.imgAsc.Visibility = Visibility.Collapsed;
                this.imgDesc.Visibility = Visibility.Collapsed;
            }
            // 刷新页面
            this.RefreshView();
        }

        private void cbSort_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(e.AddedItems.Count == 0 || this.valueLabelList.Count == 0)
            {
                return;
            }
            this.sortOrder =  (e.AddedItems[0] as FrameworkElement).Tag.ToString();
            if (this.sortOrder == "ASC")
            {
                this.imgAsc.Visibility = Visibility.Visible;
                this.imgDesc.Visibility = Visibility.Collapsed;
            }
            else if (this.sortOrder == "DESC")
            {
                this.imgAsc.Visibility = Visibility.Collapsed;
                this.imgDesc.Visibility = Visibility.Visible;
            }
            else
            {
                this.imgAsc.Visibility = Visibility.Collapsed;
                this.imgDesc.Visibility = Visibility.Collapsed;
            }
            // 刷新页面
            this.RefreshView();
        }
    }
}

using BatteryPower.Helpers;
using BatteryPower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
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
    /// Interaction logic for ReportView.xaml
    /// </summary>
    public partial class ReportView : UserControl
    {
        private DataTable dataTable = null;
        private string batteryFile
        {
            get { return Param.BATTERY_FILE; }
        }
        private ObservableCollection<Battery> batteryList = new ObservableCollection<Battery>();
        private string dataFile
        {
            get { return Param.VOLTAGE_FILE; }
        }

        public ReportView()
        {
            InitializeComponent();

            var list = XmlHelper.LoadFromXml(this.batteryFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            //if (list != null)
            //{
            //    this.batteryList = list;
            //    for (var i = 0; i < 24; i++)
            //    {
            //        DataColumn col = new DataColumn("单体电压" + (i + 1));
            //        dataTable.Columns.Add(col);
            //    }
            //}

            // 数据表结构
            dataTable = CSVFileHelper.OpenCSV(dataFile);
            if (dataTable == null)
            {
                dataTable = new DataTable();
                DataColumn dc = new DataColumn("采集时间");
                dataTable.Columns.Add(dc);
                DataColumn dc2 = new DataColumn("地址");
                dataTable.Columns.Add(dc2);

                for (var i = 0; i < 24; i++)
                {
                    DataColumn col = new DataColumn("单体电压" + (i + 1));
                    dataTable.Columns.Add(col);
                }
                CSVFileHelper.SaveCSV(dataTable, dataFile);
            }
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            if(dpStart.SelectedDate.HasValue && dpEnd.SelectedDate.HasValue && dpEnd.SelectedDate.Value < dpStart.SelectedDate.Value)
            {
                MessageBox.Show("结束日期不能小于开始日期！");
            }


            string rowFilter = "1=1";
            if (!string.IsNullOrEmpty(this.tbAddress.Text))
            {
                rowFilter += " AND 地址 = " + this.tbAddress.Text;
            }
            if (dpStart.SelectedDate.HasValue)
            {
                var dateStart = dpStart.SelectedDate.Value;
                rowFilter += " AND 采集时间  >= #" + dateStart + "#";
            }
            if (dpEnd.SelectedDate.HasValue)
            {
                var dateEnd = dpEnd.SelectedDate.Value.AddDays(1);
                rowFilter += " AND 采集时间  <= #" + dateEnd + "#";
            }

            dataTable.DefaultView.RowFilter = rowFilter;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

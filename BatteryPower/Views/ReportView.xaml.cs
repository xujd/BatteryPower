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
        private DataTable dataTableToShow = null;
        private string batteryFile
        {
            get { return Param.BATTERY_FILE; }
        }
        private List<Battery> batteryList = new List<Battery>();
        private string dataFile
        {
            get { return Param.VOLTAGE_FILE; }
        }

        public ReportView()
        {
            InitializeComponent();

            var list = XmlHelper.LoadFromXml(this.batteryFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            if (list != null)
            {
                this.batteryList = list.Where(i => i.isEnabled == "是").OrderBy(i => i.uid).ToList();
            }

            this.CreateShowTable();

            // 数据表结构
            this.ReadDataFromStore();
        }

        private void ReadDataFromStore()
        {
            dataTableToShow.Clear();
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

            if (this.batteryList.Count > 0)
            {
                List<string> addrList = new List<string>();
                foreach(var b in this.batteryList)
                {
                    addrList.Add("'" + b.address + "'");
                }
                while (dataTable.Select("地址 IN (" + string.Join(",", addrList) + ")").Count() > 0)
                {
                    var time = "";
                    foreach (var b in this.batteryList)
                    {
                        var dr = dataTable.Select("地址 = " + b.address);
                        if (dr != null && dr.Count() > 0)
                        {
                            time = dr[0]["采集时间"].ToString();
                            break;
                        }
                    }

                    if(!string.IsNullOrEmpty(time))
                    {
                        DataRow newRow = dataTableToShow.NewRow();
                        newRow["采集时间"] = time;
                        // 前后一分钟的数据为同一批次数据
                        var dateStart = DateTime.Parse(time).AddMinutes(-1);
                        var dateEnd = DateTime.Parse(time).AddMinutes(1);
                        int index = 0;
                        foreach (var b in this.batteryList)
                        {
                            var drs = dataTable.Select("地址 = " + b.address + " AND 采集时间 >= #" + dateStart + "# AND 采集时间 <= #" + dateEnd + "#");
                            if (drs != null && drs.Count() > 0)
                            {
                                var dr = drs[0];
                                for (var i = 2; i < dr.ItemArray.Length; i++)
                                {
                                    newRow[++index] = dr[i];
                                }
                                dataTable.Rows.Remove(dr);
                            }
                            else
                            {
                                for (var i = 0; i < 24; i++)
                                {
                                    newRow[++index] = null;
                                }
                            }
                        }

                        dataTableToShow.Rows.Add(newRow);
                    }
                }
            }
        }

        private void CreateShowTable()
        {
            dataTableToShow = new DataTable();
            DataColumn dc = new DataColumn("采集时间");
            dataTableToShow.Columns.Add(dc);

            int len = this.batteryList.Count * 24;
            for (var i = 0; i < len; i++)
            {
                DataColumn col = new DataColumn("单体电压" + (i + 1));
                dataTableToShow.Columns.Add(col);
            }
            dataGrid.ItemsSource = dataTableToShow.DefaultView;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            if(dpStart.SelectedDate.HasValue && dpEnd.SelectedDate.HasValue && dpEnd.SelectedDate.Value < dpStart.SelectedDate.Value)
            {
                MessageBox.Show("结束日期不能小于开始日期！");
                return;
            }


            string rowFilter = "1=1";
            //if (!string.IsNullOrEmpty(this.tbAddress.Text))
            //{
            //    rowFilter += " AND 地址 = " + this.tbAddress.Text;
            //}
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

            this.ReadDataFromStore();

            dataTableToShow.DefaultView.RowFilter = rowFilter;
        }

        private void btnExport_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}

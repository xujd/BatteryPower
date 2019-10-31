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
        private string dataFile
        {
            get { return Param.APPFILEPATH + @"data\batterys.xml"; }
        }
        public ReportView()
        {
            InitializeComponent();
            dataTable = new DataTable();
            DataColumn dc = new DataColumn("采集时间");
            dataTable.Columns.Add(dc);

            var list = XmlHelper.LoadFromXml(this.dataFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            if (list != null)
            {
                foreach (var item in list)
                {
                    DataColumn col = new DataColumn("电池" + item.id);
                    dataTable.Columns.Add(col);
                }
            }
            dataGrid.ItemsSource = dataTable.DefaultView;
        }

        private void btnQuery_Click(object sender, RoutedEventArgs e)
        {
            if(dpStart.SelectedDate.HasValue && dpEnd.SelectedDate.HasValue && dpEnd.SelectedDate.Value < dpStart.SelectedDate.Value)
            {
                MessageBox.Show("结束日期不能小于开始日期！");
            }
        }
    }
}

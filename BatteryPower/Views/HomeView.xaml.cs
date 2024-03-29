﻿using BatteryPower.Helpers;
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
using Visifire.Charts;

namespace BatteryPower.Views
{
    /// <summary>
    /// Interaction logic for HomeView.xaml
    /// </summary>
    public partial class HomeView : UserControl
    {
        private string dataFile
        {
            get { return Param.BATTERY_FILE; }
        }
        private ObservableCollection<Battery> batteryList = new ObservableCollection<Battery>();
        private string portFile
        {
            get { return Param.PORT_FILE; }
        }

        private PortConfig portConfig = null;

        private Boolean isDoing = false;

        private TaskRT task = null;

        public HomeView()
        {
            InitializeComponent();

            this.init();

            this.Unloaded += HomeView_Unloaded;
        }

        private void init()
        {
            var list = XmlHelper.LoadFromXml(this.dataFile, typeof(ObservableCollection<Battery>)) as ObservableCollection<Battery>;
            if (list != null)
            {
                this.batteryList = list;
            }

            var config = XmlHelper.LoadFromXml(this.portFile, typeof(PortConfig)) as PortConfig;
            if (config != null)
            {
                this.portConfig = config;
            }

            chart1.Series.Clear();
            var ds = new Visifire.Charts.DataSeries();
            ds.RenderAs = Visifire.Charts.RenderAs.Column;
            ds.LabelEnabled = true;
            ds.DataPoints.Add(new Visifire.Charts.DataPoint() { AxisXLabel = "未启动任务", YValue = this.batteryList.Where(i => i.isEnabled == "否").Count() });
            ds.DataPoints.Add(new Visifire.Charts.DataPoint() { AxisXLabel = "已启动任务", YValue = this.batteryList.Where(i => i.isEnabled == "是").Count() });
            chart1.Series.Add(ds);

            tbTotal.Text = this.batteryList.Count.ToString();
            tbDisabled.Text = this.batteryList.Where(i => i.isEnabled == "否").Count().ToString();
            tbEnabled.Text = this.batteryList.Where(i => i.isEnabled == "是").Count().ToString();
            // 串口配置
            if (this.portConfig != null)
            {
                tbPort.Text = this.portConfig.serialName;
                tbBitRate.Text = this.portConfig.baudRate;
                tbDataBit.Text = this.portConfig.dataBit;
                tbStopBit.Text = this.portConfig.stopBit;
                tbParity.Text = this.portConfig.parityBit;
            }
        }

        private void HomeView_Unloaded(object sender, RoutedEventArgs e)
        {
            this.StopTask();
        }

        public void StopTask()
        {
            if (this.task != null)
            {
                this.task.Stop();
                this.task = null;
            }
        }

        private bool Check()
        {
            if (string.IsNullOrEmpty(this.tbBarrery.Text))
            {
                LogHelper.WriteLog(LogType.ERROR, "任务信息不全，蓄电池编号不能为空！");
                return false;
            }
            Param.OPERATOR_INFO.BatteryNo = this.tbBarrery.Text;

            if (string.IsNullOrEmpty(this.tbOprUser.Text))
            {
                LogHelper.WriteLog(LogType.ERROR, "任务信息不全，操作人员不能为空！");
                return false;
            }
            Param.OPERATOR_INFO.OprUser = this.tbOprUser.Text;

            if (string.IsNullOrEmpty(this.tbTrainNo.Text))
            {
                LogHelper.WriteLog(LogType.ERROR, "任务信息不全，机车号不能为空！");
                return false;
            }
            Param.OPERATOR_INFO.TrainNo = this.tbTrainNo.Text;

            double cycle = 5;
            if (!double.TryParse(tbCycle.Text, out cycle))
            {
                LogHelper.WriteLog(LogType.ERROR, "任务信息错误，采集周期为非法数字！");
                return false;
            }
            if (cycle < 5)
            {
                LogHelper.WriteLog(LogType.ERROR, "任务信息错误，采集周期最小为5分钟！");
                return false;
            }
            Param.OPERATOR_INFO.StoreCycle = cycle;

            return true;
        }

        private void btnStart_Click(object sender, RoutedEventArgs e)
        {
            if (!this.Check())
            {
                return;
            }
            this.isDoing = !this.isDoing;
            this.btnStart.Content = this.isDoing ? "停止任务" : "启动任务";
            if (this.isDoing)
            {
                this.task = new TaskRT(this.batteryList.ToList(), this.portConfig);
                this.task.Start();
            }
            else
            {
                if (this.task != null)
                {
                    this.task.Stop();
                    this.task = null;
                }
            }

            this.btnSync.IsEnabled = !this.isDoing;
        }

        private void btnSync_Click(object sender, RoutedEventArgs e)
        {
            this.init();
        }
    }
}

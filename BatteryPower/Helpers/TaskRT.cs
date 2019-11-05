using BatteryPower.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace BatteryPower.Helpers
{
    public class TaskRT
    {
        private CancellationTokenSource cts = new CancellationTokenSource();
        private SerialPort curSerialPort = null;
        private PortConfig portConfig = null;
        private List<Battery> batteryList = null;

        private bool isStoped = false;
        private Thread thread = null;
        // 当前处理的索引
        private volatile int processingIndex = 0;
        // 需要处理的任务
        private volatile List<Battery> processingList = new List<Battery>();
        // 是否进行中
        private volatile bool isDoing = false;
        // 定时器
        private DispatcherTimer checkTimer = new DispatcherTimer();
        private int timeIndex = 0;
        private int maxIndex = 1;
        // 数据
        private DataTable dataTable = null;
        private string dataFile
        {
            get { return Param.VOLTAGE_FILE; }
        }

        List<Object[]> lastestData = new List<object[]>();
        int timeTick = 0;
        bool isFirstSave = true;

        public TaskRT(List<Battery> batteryList, PortConfig portConfig)
        {
            this.batteryList = batteryList;
            this.portConfig = portConfig;

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
        }

        public void Start()
        {
            if (batteryList == null || batteryList.Where(i => i.isEnabled == "是" && i.collectCycle > 1).Count() == 0)
            {
                LogHelper.WriteLog(LogType.ERROR, "可执行任务列表为空，请先添加资源并在采集管理中启动任务。");
                return;
            }
            if (portConfig == null || string.IsNullOrEmpty(portConfig.serialName))
            {
                LogHelper.WriteLog(LogType.ERROR, "串口未配置，请先在巡检调试中选择串口。");
                return;
            }
            if (!this.CreateSerialPort(portConfig.serialName, portConfig.baudRate, portConfig.dataBit, portConfig.stopBit, portConfig.parityBit))
            {
                LogHelper.WriteLog(LogType.ERROR, "串口打开失败...");
                return;
            }
            LogHelper.WriteLog(LogType.INFO, "任务已启动...");
            var list = batteryList.Where(i => i.isEnabled == "是" && i.collectCycle > 1);

            List<int> cycleSpans = new List<int>();
            foreach (var item in list)
            {
                var b = new Battery();
                b.id = item.id;
                b.address = item.address;
                b.coefficient = item.coefficient;
                cycleSpans.Add(item.collectCycle);
                processingList.Add(b);
                LogHelper.WriteLog(LogType.INFO, "添加到待执行任务队列，地址为：" + b.address);
            }

            this.maxIndex = MathHelper.LeastCommonMultiple(cycleSpans);

            thread = new Thread(new ThreadStart(startTask));
            thread.Start();

            // 定时任务，5秒执行一次
            checkTimer.Interval = TimeSpan.FromSeconds(5);
            checkTimer.Tick += CheckTimer_Tick;
            checkTimer.Start();
            
        }

        private void CheckTimer_Tick(object sender, EventArgs e)
        {
            //if (timeIndex == this.maxIndex)
            //{
            //    timeIndex = 0;
            //}
            //++timeIndex;
            var list = batteryList.Where(i => i.isEnabled == "是" && i.collectCycle > 1);

            foreach (var item in list)
            {
                if (processingList.Find(i => i.address == item.address) == null)  // 上一轮执行完时才添加下一轮
                {
                    var b = new Battery();
                    b.id = item.id;
                    b.address = item.address;
                    processingList.Add(b);
                    LogHelper.WriteLog(LogType.INFO, "添加到待执行任务队列，地址为：" + b.address);
                    //if (timeIndex % item.collectCycle == 0)  // 处理时间到
                    //{
                    //    processingList.Add(b);
                    //    LogHelper.WriteLog(LogType.INFO, "添加到待执行任务队列，地址为：" + b.address);
                    //}
                }
            }

            if (timeTick++ >= 60 || isFirstSave)  // 每5分钟保存到存储
            {
                if (!isFirstSave)
                {
                    timeTick = 0;
                }
                isFirstSave = false;
                // 保存到本地
                this.SaveToStore();
            }

        }

        private void SaveToStore()
        {
            var list = batteryList.Where(i => i.isEnabled == "是" && i.collectCycle > 1);
            foreach (var item in list)
            {
                var curObj = this.lastestData.FindLast(i => i[1].ToString() == item.address);
                if (curObj != null)
                {
                    DataRow dr = dataTable.NewRow();
                    for (var i = 0; i < curObj.Count(); i++)
                    {
                        dr[i] = curObj[i];
                    }
                    dataTable.Rows.Add(dr);
                }
            }
            CSVFileHelper.SaveCSV(dataTable, dataFile);
        }

        private void startTask()
        {
            while (true)
            {
                Thread.Sleep(200);
                if (this.isStoped)
                {
                    break;
                }
                if (cts.Token.IsCancellationRequested)
                {
                    Console.WriteLine("线程被终止！");
                    break;
                }
                if (this.isDoing)
                {
                    continue;
                }
                try
                {
                    if (this.processingIndex < this.processingList.Count) // 执行下一个任务
                    {
                        this.isDoing = true;
                        LogHelper.WriteLog(LogType.INFO, "开始执行任务，地址为：" + this.processingList[this.processingIndex].address);
                        // 发送到串口
                        this.SendMessage(FormatRequest(this.processingList[this.processingIndex].address));
                        // 移除当前任务
                        this.processingList.RemoveAt(this.processingIndex);
                    }
                    else
                    {
                        this.processingIndex = 0;
                    }
                }
                catch (Exception ex)
                {
                    LogHelper.WriteLog(LogType.ERROR, string.Format("ERROR：线程处理错误：{0},{1},{2}", ex.Message, ex.Source, ex.StackTrace));
                }
            }
        }

        private string FormatRequest(string address)
        {
            string msg = address + " 03 00 00 00 30";
            var crcStr = CRC.ToModbusCRC16(msg, true);
            return msg + " " + crcStr.Substring(0, 2) + " " + crcStr.Substring(2);
        }

        public void Stop()
        {
            if (!this.isStoped)
            {
                checkTimer.Tick -= CheckTimer_Tick;
                checkTimer.Stop();
                this.CloseSerialPort();
                this.isStoped = true;
                this.isDoing = false;
                this.timeTick = 0;
                this.isFirstSave = true;
                this.processingList.Clear();
                this.processingIndex = 0;
                this.cts.Cancel();
                LogHelper.WriteLog(LogType.INFO, "任务已停止！");
            }
        }

        private void SendMessage(string message)
        {
            LogHelper.WriteLog(LogType.INFO, "开始发送数据，数据内容：" + message);
            //启动检查
            //处理数字转换
            string[] strArray = message.Split(' ');

            int byteBufferLength = strArray.Length;
            for (int i = 0; i < strArray.Length; i++)
            {
                if (strArray[i] == "")
                {
                    byteBufferLength--;
                }
            }
            // int temp = 0;
            byte[] byteBuffer = new byte[byteBufferLength];
            int ii = 0;
            for (int i = 0; i < strArray.Length; i++)//对获取的字符做相加运算
            {

                Byte[] bytesOfStr = Encoding.Default.GetBytes(strArray[i]);

                int decNum = 0;
                if (strArray[i] == "")
                {
                    continue;
                }
                else
                {
                    decNum = Convert.ToInt32(strArray[i], 16); //atrArray[i] == 12时，temp == 18 
                }

                try    //防止输错，使其只能输入一个字节的字符
                {
                    byteBuffer[ii] = Convert.ToByte(decNum);
                }
                catch (System.Exception ex)
                {
                    LogHelper.WriteLog(LogType.ERROR, "发送失败，错误信息：" + ex.Message);
                }

                ii++;
            }

            LogHelper.WriteLog(LogType.INFO, "格式化发送内容完毕，开始写入串口！");
            try
            {
                if (this.curSerialPort != null && this.curSerialPort.IsOpen)
                {
                    this.curSerialPort.Write(byteBuffer, 0, byteBuffer.Length);
                    LogHelper.WriteLog(LogType.INFO, "发送数据完毕！");
                }
                else
                {
                    LogHelper.WriteLog(LogType.WARN, "连接已关闭！");
                }
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(LogType.ERROR, "发送失败，错误信息：" + ex.Message);
            }
        }

        private bool CreateSerialPort(string serialName, string strBaudRate, string strDataBit, string strStopBit, string strParityBit)
        {
            LogHelper.WriteLog(LogType.INFO, "准备打开" + serialName + "，波特率：" + strBaudRate + "，数据位：" + strDataBit + "，停止位：" + strStopBit + "，校验位：" + strParityBit + "。");
            //创建新串口
            try
            {
                this.CloseSerialPort();

                var sp = new SerialPort();

                sp.DtrEnable = true;
                sp.RtsEnable = true;
                //设置数据读取超时为1秒
                sp.ReadTimeout = 1000;
                sp.DataReceived += Sp_DataReceived; ;

                //设置串口号
                sp.PortName = serialName;
                //设置各“串口设置”
                Int32 iBaudRate = Convert.ToInt32(strBaudRate);
                Int32 iDataBit = Convert.ToInt32(strDataBit);

                sp.BaudRate = iBaudRate;       //波特率
                sp.DataBits = iDataBit;       //数据位
                switch (strStopBit)            //停止位
                {
                    case "1":
                        sp.StopBits = StopBits.One;
                        break;
                    case "1.5":
                        sp.StopBits = StopBits.OnePointFive;
                        break;
                    case "2":
                        sp.StopBits = StopBits.Two;
                        break;
                    default:
                        break;
                }
                switch (strParityBit)             //校验位
                {
                    case "无":
                        sp.Parity = Parity.None;
                        break;
                    case "奇校验":
                        sp.Parity = Parity.Odd;
                        break;
                    case "偶校验":
                        sp.Parity = Parity.Even;
                        break;
                    default:
                        break;
                }

                if (sp.IsOpen)//如果打开状态，则先关闭一下
                {
                    sp.Close();
                }

                sp.Open();     //打开串口

                this.curSerialPort = sp;

                LogHelper.WriteLog(LogType.INFO, "打开串口成功！");

                return true;
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(LogType.ERROR, ex.Message);
                return false;
            }
        }

        private void Sp_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = sender as SerialPort;
            if (sp != null && sp.IsOpen)
            {
                Thread.Sleep(500);
                int totalLen = sp.BytesToRead;
                Byte[] receivedData = new Byte[sp.BytesToRead]; // 创建接收字节数组
                int recvLen = 0;
                while ((recvLen += sp.Read(receivedData, 0, receivedData.Length)) < totalLen) // 读取数据
                {
                    Thread.Sleep(500);
                }
                string strRcv = "";
                List<string> strList = new List<string>();
                for (int i = 0; i < receivedData.Length; i++)
                {
                    if (receivedData[i] == Byte.MaxValue) // 去掉255字符
                    {
                        continue;
                    }
                    strList.Add(receivedData[i].ToString("x2"));
                    // strRcv += receivedData[i].ToString("x2") + " ";
                }
                strRcv = string.Join(" ", strList);
                LogHelper.WriteLog(LogType.INFO, string.Format("收到信息！信息内容：{0}", strRcv));

                this.ProcessMessage(strRcv);

                // 处理下一个
                this.processingIndex++;
                // 本次处理结束
                this.isDoing = false;
            }
        }

        private void ProcessMessage(string message)
        {
            var datas = message.Split(' '); // 前三个为地址+功能码+应答字节数，后两个为CRC校验
            if (datas.Length != 101)  // 格式不对
            {
                LogHelper.WriteLog(LogType.ERROR, "数据格式不对，跳过此段数据！");
                return;
            }
            LogHelper.WriteLog(LogType.INFO, string.Format("开始处理地址{0}的数据...", datas[0]));
            var tempBattery = this.batteryList.Find(i => i.address == datas[0]);
            // 电压参考系数
            var coefficient = 0.0;
            if (tempBattery != null)
            {
                coefficient = tempBattery.coefficient;
            }
            if (coefficient < 0.001)
            {
                coefficient = 1.0;
            }
            // 电压数据
            List<double> voltageList = new List<double>();
            for (var i = 3; i < 51; i += 2)
            {
                var hexStr = datas[i] + datas[i + 1];
                var vNum = Int32.Parse(hexStr, System.Globalization.NumberStyles.HexNumber);
                // 乘以参考系数并放大1000倍
                voltageList.Add(vNum * coefficient / 1000.0);
            }

            // 保存到本地csv
            var curObj = new List<object>();
            var curTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            curObj.Add(curTime);
            curObj.Add(datas[0]);
            for (int j = 0; j < voltageList.Count; j++)
            {
                curObj.Add(voltageList[j]);
            }
            for (var j = 0; j < Param.CURRENT_VOLTAGE_DATA.Count; j++)
            {
                if (Param.CURRENT_VOLTAGE_DATA[j][1].ToString() == datas[0])
                {
                    Param.CURRENT_VOLTAGE_DATA.RemoveAt(j);
                    break;
                }
            }
            Param.CURRENT_VOLTAGE_DATA.Add(curObj.ToArray());

            // 缓存数据
            this.lastestData.Add(curObj.ToArray());
            if (this.lastestData.Count > 100)
            {
                this.lastestData.RemoveAt(0);
            }

            LogHelper.WriteLog(LogType.ERROR, "数据处理完毕！采集到的电压为：" + string.Join(" ", voltageList));
        }

        private void CloseSerialPort()
        {
            if (this.curSerialPort != null && this.curSerialPort.IsOpen)
            {
                this.curSerialPort.DiscardInBuffer();
                this.curSerialPort.DiscardOutBuffer();
                this.curSerialPort.Dispose();
                this.curSerialPort.Close();
                this.curSerialPort.DataReceived -= Sp_DataReceived;
                this.curSerialPort = null;
            }
        }
    }
}

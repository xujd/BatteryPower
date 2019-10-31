using BatteryPower.Helpers;
using BatteryPower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
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

namespace BatteryPower.Views
{
    /// <summary>
    /// Interaction logic for DebugView.xaml
    /// </summary>
    public partial class DebugView : UserControl
    {
        private SerialPort curSerialPort = null;
        List<string> bitRateList = new List<string>() { "300", "600", "1200", "2400", "4800", "9600", "19200", "38400", "115200" };
        List<string> dataBitList = new List<string>() { "5", "6", "7", "8" };
        List<string> stopBitList = new List<string>() { "1", "1.5", "2" };
        List<string> parityList = new List<string>() { "无", "奇校验", "偶校验" };
        List<string> portNames = new List<string>();

        public DebugView()
        {
            InitializeComponent();

            this.init();
        }

        private void init()
        {
            this.portNames = SerialPort.GetPortNames().ToList();
            this.cbSerialPort.ItemsSource = this.portNames;
            this.cbSerialPort.SelectedIndex = 0;
            this.cbBitRate.ItemsSource = bitRateList;
            this.cbBitRate.SelectedIndex = 5;//默认9600
            this.cbDataBit.ItemsSource = dataBitList;
            this.cbDataBit.SelectedIndex = 3;//默认8
            this.cbStopBit.ItemsSource = stopBitList;
            this.cbStopBit.SelectedIndex = 0;//默认1
            this.cbParity.ItemsSource = parityList;
            this.cbParity.SelectedIndex = 1;//默认奇校验

            this.lbLog.ItemsSource = LogHelper.LogList;
        }

        private void btnOpenPort_Click(object sender, RoutedEventArgs e)
        {
            if (this.cbSerialPort.SelectedItem == null)
            {
                LogHelper.WriteLog(LogType.ERROR, "没有选择串口！");
                return;
            }

            if (this.CreateSerialPort(this.cbSerialPort.SelectedItem.ToString(), this.cbBitRate.SelectedItem.ToString(),
                this.cbDataBit.SelectedItem.ToString(), this.cbStopBit.SelectedItem.ToString(), this.cbParity.SelectedItem.ToString()))
            {
                this.cbSerialPort.IsEnabled = this.cbBitRate.IsEnabled = this.cbDataBit.IsEnabled
                    = this.cbParity.IsEnabled
                    = this.cbStopBit.IsEnabled = false;
                this.btnOpenPort.IsEnabled = false;
                this.btnClosePort.IsEnabled = true;

                this.tbMessage.IsEnabled = this.btnSend.IsEnabled = true;
            }
        }

        private void btnClosePort_Click(object sender, RoutedEventArgs e)
        {
            this.CloseSerialPort();

            this.cbSerialPort.IsEnabled = this.cbBitRate.IsEnabled = this.cbDataBit.IsEnabled
                = this.cbParity.IsEnabled
                = this.cbStopBit.IsEnabled = true;
            this.btnOpenPort.IsEnabled = true;
            this.btnClosePort.IsEnabled = false;
            this.tbMessage.IsEnabled = this.btnSend.IsEnabled = false;
        }

        private void CloseSerialPort()
        {
            if (this.curSerialPort != null && this.curSerialPort.IsOpen)
            {
                this.curSerialPort.DiscardInBuffer();
                this.curSerialPort.DiscardOutBuffer();
                this.curSerialPort.Dispose();
                this.curSerialPort.Close();
                this.curSerialPort = null;
            }
        }

        private bool CreateSerialPort(string serialName, string strBaudRate, string strDateBits, string strStopBits, string strParity)
        {
            LogHelper.WriteLog(LogType.INFO, "准备打开" + serialName + "，波特率：" + strBaudRate + "，数据位：" + strDateBits + "，停止位：" + strStopBits + "，校验位：" + strParity + "。");
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
                Int32 iDateBits = Convert.ToInt32(strDateBits);

                sp.BaudRate = iBaudRate;       //波特率
                sp.DataBits = iDateBits;       //数据位
                switch (strStopBits)            //停止位
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
                switch (strParity)             //校验位
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
                for (int i = 0; i < receivedData.Length; i++)
                {
                    if (receivedData[i] == Byte.MaxValue) // 去掉255字符
                    {
                        continue;
                    }
                    strRcv += receivedData[i].ToString("x2") + " ";
                }
                LogHelper.WriteLog(LogType.INFO, string.Format("收到信息！信息内容：{0}", strRcv));
            }
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            if (this.curSerialPort == null || !this.curSerialPort.IsOpen)
            {
                LogHelper.WriteLog(LogType.ERROR, "串口通信连接未建立！");
                return;
            }
            if (string.IsNullOrEmpty(tbMessage.Text))
            {
                LogHelper.WriteLog(LogType.ERROR, "发送数据不能为空！");
                return;
            }
            LogHelper.WriteLog(LogType.INFO, "开始发送数据，数据内容：" + tbMessage.Text);
            //启动检查
            //处理数字转换
            string[] strArray = tbMessage.Text.Split(' ');

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
                this.curSerialPort.Write(byteBuffer, 0, byteBuffer.Length);
            }
            catch (System.Exception ex)
            {
                LogHelper.WriteLog(LogType.ERROR, "发送失败，错误信息：" + ex.Message);
            }
            LogHelper.WriteLog(LogType.INFO, "写入内容完毕！");
        }
    }
}

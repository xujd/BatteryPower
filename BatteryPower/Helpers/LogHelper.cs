using BatteryPower.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Threading;

namespace BatteryPower.Helpers
{
    class LogHelper
    {
        public static ObservableCollection<string> LogList = null;

        public static Dispatcher dispatcher = null;
        private static Thread thread = null;
        private static bool isStoped = false;
        private static volatile List<LogModel> logQueue = new List<LogModel>();

        static LogHelper()
        {
            LogList = new ObservableCollection<string>();
            thread = new Thread(new ThreadStart(StartLog));
            thread.Start();
        }

        public delegate void CleanDelegate();
        public delegate void LogDelegate(string log);
        public static void Clean()
        {
        }

        public static void Stop()
        {
            isStoped = true;
        }

        private static void StartLog()
        {
            while (true)
            {
                Thread.Sleep(100);
                if (isStoped)
                {
                    break;
                }

                if (logQueue.Count > 0)
                {
                    Write(logQueue[0].type, logQueue[0].message, logQueue[0].path, logQueue[0].flag);
                    logQueue.RemoveAt(0);
                }
            }
        }

        private static void Write(LogType type, string message, string subdir = @"\log\", bool flag = false)
        {
            try
            {
                string path = Param.APPFILEPATH + subdir + DateTime.Now.ToString("yyyy-MM") + @"\";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                path += DateTime.Now.ToString("yyyyMMdd") + ".txt";
                //if (!File.Exists(path))
                //{
                //    File.Create(path);
                //}

                string log = type + " " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + message;

                FileStream fs;
                StreamWriter sw;
                fs = new FileStream(path, FileMode.Append);
                sw = new StreamWriter(fs, Encoding.Default);
                sw.Write(log + "\r\n");
                sw.Close();
                sw.Dispose();
                fs.Close();
                fs.Dispose();

                LogHelper.dispatcher.Invoke(new Action(() =>
                {
                    LogList.Insert(0, log);
                    if (LogList.Count > 150)//保留最新100条记录
                    {
                        while (LogList.Count > 100)
                        {
                            LogList.RemoveAt(LogList.Count - 1);
                        }
                    }
                }));

                //Dispatcher.CurrentDispatcher.BeginInvoke(Log.InvokeToList());
            }
            catch (Exception e)
            {
                if (!flag)
                {
                    WriteLog(LogType.ERROR, "程序发生异常（WriteLog）。详情：" + e.Message, @"\log\", true);
                }
            }
        }

        public static void WriteLog(LogType type, string message, string subdir = @"\log\", bool flag = false)
        {
            var log = new LogModel(type, message, subdir, flag);

            logQueue.Add(log);
        }

        private static void InvokeToList()
        {
            string log = "";
            LogList.Insert(0, log);
            if (LogList.Count > 150)//保留最新100条记录
            {
                while (LogList.Count > 100)
                {
                    LogList.RemoveAt(LogList.Count - 1);
                }
            }
        }
    }

    class LogModel
    {
        public LogType type;
        public string message;
        public string path;
        public bool flag;

        public LogModel(LogType type, string message, string path, bool flag)
        {
            this.type = type;
            this.message = message;
            this.path = path;
            this.flag = flag;
        }
    }
}

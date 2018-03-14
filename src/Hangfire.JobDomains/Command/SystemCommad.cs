using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Command
{
    internal class SystemCommad
    {

        static bool IsRunning = false;

        static object runningLock = 1;

        public static Task<bool> Exec(string cmd)
        {
            if (IsRunning == false)
            {
                lock (runningLock)
                {
                    if (IsRunning == false)
                    {
                        IsRunning = true;
                        return Task.Run(() =>
                       {
                           System.Diagnostics.Process p = new System.Diagnostics.Process();
                           p.StartInfo.FileName = "cmd.exe";
                           p.StartInfo.WorkingDirectory = AppDomain.CurrentDomain.BaseDirectory;
                           p.StartInfo.UseShellExecute = false;        //是否使用操作系统shell启动
                            p.StartInfo.RedirectStandardInput = true;   //接受来自调用程序的输入信息
                            p.StartInfo.RedirectStandardOutput = false; //由调用程序获取输出信息
                            p.StartInfo.RedirectStandardError = true;   //重定向标准错误输出
                            p.StartInfo.CreateNoWindow = true;          //不显示程序窗口
                            p.Start();                                  //启动程序
                            p.StandardInput.WriteLine(cmd + "&exit");   //向cmd窗口发送输入信息
                            p.StandardInput.WriteLine("");              //向cmd窗口发送输入信息
                            p.WaitForExit();                            //等待程序执行完退出进程
                            p.Close();
                           IsRunning = false;
                           return true;
                       });
                    }
                }
            }

            return Task.FromResult(false);
        }
    }
}

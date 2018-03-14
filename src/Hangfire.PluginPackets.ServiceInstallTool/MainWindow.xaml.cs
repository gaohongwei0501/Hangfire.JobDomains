using Common.Logging;
using Hangfire.PluginPackets.Config;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Hangfire.PluginPackets.ServiceInstallTool
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {

        private static readonly ILog _logger = Common.Logging.LogManager.GetLogger<MainWindow>();

        public ServiceInstallModel ServiceInstallEntity { get; set; }

        public bool IsRunning = false;

        public MainWindow()
        {
            InitializeComponent();
            CheckAdministrator();
            RefreshCmdState(false);
        }

        /// <summary>  
        /// 检查是否是管理员身份  
        /// </summary>  
        private void CheckAdministrator()
        {
            var wi = WindowsIdentity.GetCurrent();
            var wp = new WindowsPrincipal(wi);

            bool runAsAdmin = wp.IsInRole(WindowsBuiltInRole.Administrator);

            if (!runAsAdmin)
            {
                // It is not possible to launch a ClickOnce app as administrator directly,  
                // so instead we launch the app as administrator in a new process.  
                var processInfo = new ProcessStartInfo(Assembly.GetExecutingAssembly().CodeBase);

                // The following properties run the new process as administrator  
                processInfo.UseShellExecute = true;
                processInfo.Verb = "runas";

                // Start the new process  
                try
                {
                    Process.Start(processInfo);
                }
                catch (Exception ex)
                {
                    _logger.Error(ex);
                }

                // Shut down the current process  
                Environment.Exit(0);
            }
        }
        
        private void Button_SelectInstallFile_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Win32.OpenFileDialog ofd = new Microsoft.Win32.OpenFileDialog();
            ofd.DefaultExt = ".exe";
            ofd.Filter = "exe file|*.exe";
            if (ofd.ShowDialog() == true && string.IsNullOrEmpty(ofd.FileName) == false)
            {
                ServiceInstallFile.Text= ofd.FileName;
                GetService(ofd.FileName);
                RefreshServiceState();
            }
        }

        void GetService(string fileName)
        {
            if (File.Exists(fileName) == false) return;
            var file = new FileInfo(fileName);
            var path = file.Directory.FullName;
            var setting = new ServiceSettings(path, "Hangfire.PluginPackets.Service");
            var value = setting.GetValue();

            ServiceInstallEntity = new ServiceInstallModel
            {
                FileName = file.Name,
                Directory = path,
                ServiceName = value.ServiceName,
                InstallState = false,
                RunningState = false
            };
        }

        private async void Button_Install_Click(object sender, RoutedEventArgs e)
        {
            await Run(InstallService);
        }

        private async void Button_Uninstall_Click(object sender, RoutedEventArgs e)
        {
            await Run(UninstallService);
        }

        private async void Button_Start_Click(object sender, RoutedEventArgs e)
        {
            await Run(StartService);
        }

        private async void Button_Stop_Click(object sender, RoutedEventArgs e)
        {
            await Run(StopService);
        }

        async Task<bool> Run(Func<Task<bool>> Cmd)
        {
            if (IsRunning) return ShowMessage("存在正在进行的任务", false);
            var fresh = RefreshServiceState();
            if (fresh == false) return ShowMessage("未发现服务执行程序", false);
            var state = await Cmd();
            if (state) return RefreshServiceState();
            return false;
        }

        Task Exec(string cmd)
        {
            return Task.Run(() =>
            {
                RefreshCmdState (true);

                System.Diagnostics.Process p = new System.Diagnostics.Process();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = ServiceInstallEntity.Directory;
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

                RefreshCmdState (false);
            });
        }

        async Task<bool> InstallService()
        {
            if (ServiceInstallEntity.InstallState == true) return ShowMessage("服务已安装", false);
            ShowMessage("服务开始安装...");
            await Exec($"{ServiceInstallEntity.FileName} install");
            ShowMessage("服务安装操作执行完毕");
            return true;
        }

        async Task<bool> UninstallService()
        {
            if (ServiceInstallEntity.InstallState == false) return ShowMessage("服务不存在", false);
            if (ServiceInstallEntity.RunningState == true)
            {
                ShowMessage("服务正在运行...");
                var stop = await StopService();
                if (stop == false) return false;
                RefreshServiceState();
                await UninstallService();
            }
            ShowMessage("服务卸载开始...");
            await Exec($"{ServiceInstallEntity.FileName} uninstall");
            ShowMessage("服务卸载操作执行完毕");
            return true;
        }

        async Task<bool> StartService()
        {
            if (ServiceInstallEntity.InstallState == false) return ShowMessage("服务不存在", false);
            if (ServiceInstallEntity.RunningState == true) return ShowMessage("服务已启动", false);
            ShowMessage("服务启动开始...");
            await Exec($"net  start \"{ServiceInstallEntity.ServiceName}\"");
            ShowMessage("服务启动操作执行完毕");
            return true;
        }

        async Task<bool> StopService()
        {
            if (ServiceInstallEntity.InstallState == false) return ShowMessage("服务不存在", false);
            if (ServiceInstallEntity.RunningState == false) return ShowMessage("服务已停止", false);
            ShowMessage("服务停止开始...");
            await Exec($"net  stop \"{ServiceInstallEntity.ServiceName}\"");
            ShowMessage("服务停止操作执行完毕");
            return true;
        }

        bool ShowMessage(string messsage, bool state=true)
        {
            var src = MessageBroad.Text;
            var index = src.Count(s => s == '\t');
            MessageBroad.Text = $" {index} \t {messsage} \n"+ src;
            return state;
        }

        bool RefreshCmdState(bool state) {
            IsRunning = state;
            CmdStateBroad.Dispatcher.Invoke(() => CmdStateBroad.Text = $"\t 命令行：{(state ? "被占用" : "空闲")}");
         //   CmdStateBroad.Text = $"\t 命令行：{(state ? "被占用" : "空闲")}";
            return true;
        }

        bool RefreshServiceState()
        {
            if (ServiceInstallEntity == null)
            {
                StateBroad.Text = $"\t 服务执行程序：未发现 \n\t 名称服务：未定义 \n\t 安装状态：未安装 \n\t 执行状态：未启动";
                return false;
            }
            ServiceInstallEntity.RefreshState();
            StateBroad.Text =$"\t 服务执行程序：{ServiceInstallEntity.FileName} \n\t 名称服务：{ServiceInstallEntity.ServiceName} \n\t 安装状态：{ (ServiceInstallEntity.InstallState ? "已安装" : "未安装")} \n\t 执行状态：{ (ServiceInstallEntity.RunningState? "已启动" : "未启动")}";
            return true;
        }

         
    }
}

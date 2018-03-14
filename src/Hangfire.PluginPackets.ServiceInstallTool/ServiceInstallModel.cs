using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.ServiceInstallTool
{
    public class ServiceInstallModel
    {
        public string Directory { get; set; }
        public string FileName { get; set; }
        public string ServiceName { get; set; }
        public bool InstallState { get; set; } = false;
        public bool RunningState { get; set; } = false;

        public void RefreshState()
        {
            InstallState = false;
            RunningState = false;
            ServiceController[] service = ServiceController.GetServices();
            for (int i = 0; i < service.Length; i++)
            {
                if (service[i].ServiceName.ToUpper().Equals(ServiceName.ToUpper()))
                {
                    InstallState = true;
                    if (service[i].Status == ServiceControllerStatus.Running)
                    {
                        RunningState = true;
                        break;
                    }
                }
            }
        }
    }
}

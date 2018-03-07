using Hangfire.PluginPackets.Server;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Topshelf;

namespace JobServer
{
    class Program
    {
        static void Main(string[] args)
        {
            HostFactory.Run(x =>
            {
                x.RunAsLocalSystem();
                var setting = ServiceSettings.Value.GetValue();
                x.SetServiceName(setting.ServiceName);
                x.SetDisplayName(setting.ServiceDisplayName);
                x.SetDescription(setting.ServiceDescription);

                x.Service(() => new Bootstrap());

                x.SetStartTimeout(TimeSpan.FromMinutes(5));
                //https://github.com/Topshelf/Topshelf/issues/165
                x.SetStopTimeout(TimeSpan.FromMinutes(35));

                x.EnableServiceRecovery(r => { r.RestartService(1); });
            });

            Console.ReadLine();
        }
    }
}

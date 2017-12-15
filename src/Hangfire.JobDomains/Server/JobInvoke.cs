using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Loader;
using Hangfire.JobDomains.Storage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Server
{
    internal class JobInvoke
    {
        public static void ScheduleEnqueued(TimeSpan delay, int period, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => RecurringInvoke(period, queue, pluginName, assembly, job, paramers), delay);
        }

        public static void DelayEnqueued(TimeSpan delay, string queue, string pluginName, string assembly, string job, object[] paramers )
        {
            BackgroundJob.Schedule(() => ImmediatelyEnqueued(queue, pluginName, assembly, job, paramers), delay);
        }

        public static void ImmediatelyEnqueued(string queue,string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create<JobInvoke>(c => Invoke(pluginName, assembly, job, paramers), state);
        }

        public static void RecurringInvoke(int period, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            RecurringJob.AddOrUpdate(() => Invoke(pluginName, assembly, job, paramers), Cron.MinuteInterval(period), queue: queue);
        }

        public static void Invoke(string pluginName, string assembly, string job, object[] paramers)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(pluginName);
            setup.ConfigurationFile = $"{pluginName}\\App.config";
            setup.PrivateBinPath = pluginName;
            setup.DisallowApplicationBaseProbing = false;
            setup.DisallowBindingRedirects = false;
            var Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
            try
            {
                var server = StorageService.Provider.GetServer(Environment.MachineName) ;
                var path = $"{ server.PlugPath }//{ pluginName }";
                if (Directory.Exists(path)) throw (new Exception("此服务器不支持该插件"));
                var args = new CrossDomainData { PluginDir = path, assemblyName = assembly, typeName = job, paramers = paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(PrefabricationActivator.Dispatch));
            }
            finally
            {
                AppDomain.Unload(Domain);
            }
        }

        public static bool Test(string pluginName, string assembly, string job, object[] paramers)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(pluginName);
            setup.ConfigurationFile = $"{pluginName}\\App.config";
            setup.PrivateBinPath = pluginName;
            setup.DisallowApplicationBaseProbing = false;
            setup.DisallowBindingRedirects = false;
            var Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
            try
            {
                var server = StorageService.Provider.GetServer(Environment.MachineName);
                var path = $"{ server.PlugPath }//{ pluginName }";
                if (Directory.Exists(path)) throw (new Exception("此服务器不支持该插件"));
                var args = new CrossDomainData {  PluginDir= path, assemblyName = assembly, typeName= job, paramers= paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(PrefabricationActivator.Test));
                return (bool)Domain.GetData("result");
            }
            finally {
                AppDomain.Unload(Domain);
            }
        }

    }


}

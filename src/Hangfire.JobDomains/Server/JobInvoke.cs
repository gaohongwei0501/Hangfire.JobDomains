using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Loader;
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
        public static void ScheduleEnqueued(TimeSpan delay, int period, string queue, string path, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => RecurringInvoke(period, queue, path, assembly, job, paramers), delay);
        }

        public static void DelayEnqueued(TimeSpan delay, string queue, string path, string assembly, string job, object[] paramers )
        {
            BackgroundJob.Schedule(() => ImmediatelyEnqueued(queue, path, assembly, job, paramers), delay);
        }

        public static void ImmediatelyEnqueued(string queue,string path, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create<JobInvoke>(c => Invoke(path, assembly, job, paramers), state);
        }

        public static void RecurringInvoke(int period, string queue, string path, string assembly, string job, object[] paramers)
        {
            RecurringJob.AddOrUpdate(() => Invoke(path, assembly, job, paramers), Cron.MinuteInterval(period), queue: queue);
        }

        public static void Invoke(string path, string assembly, string job, object[] paramers)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(path);
            setup.ConfigurationFile = $"{path}\\App.config";
            setup.PrivateBinPath = path;
            setup.DisallowApplicationBaseProbing = false;
            setup.DisallowBindingRedirects = false;
            var Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
            try
            {
                var args = new CrossDomainData { PluginDir = path, assemblyName = assemblyName, typeName = typeName, paramers = paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(PrefabricationActivator.Dispatch));
            }
            finally
            {
                AppDomain.Unload(Domain);
            }
        }

        public static bool Test(string path, string assemblyName, string typeName, object[] paramers)
        {
            AppDomainSetup setup = new AppDomainSetup();
            setup.ApplicationBase = Path.GetDirectoryName(path);
            setup.ConfigurationFile = $"{path}\\App.config";
            setup.PrivateBinPath = path;
            setup.DisallowApplicationBaseProbing = false;
            setup.DisallowBindingRedirects = false;
            var Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
            try
            {
                var args = new CrossDomainData {  PluginDir= path, assemblyName= assemblyName, typeName= typeName, paramers= paramers };
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

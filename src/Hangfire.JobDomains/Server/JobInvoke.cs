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
        public static void ScheduleEnqueued(TimeSpan delay, int period, string queue, string jobSign, string pluginName, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => RecurringInvoke(period, queue, jobSign, pluginName, assembly, job, paramers), delay);
        }

        public static void DelayEnqueued(TimeSpan delay, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => ImmediatelyEnqueued(queue, pluginName, assembly, job, paramers), delay);
        }

        public static void ImmediatelyEnqueued(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create<JobInvoke>(c => Invoke(pluginName, assembly, job, paramers), state);
        }

        public static void RecurringInvoke(int period, string queue, string jobSign, string pluginName, string assembly, string job, object[] paramers)
        {
            RecurringJob.AddOrUpdate(jobSign, () => Invoke(pluginName, assembly, job, paramers), Cron.MinuteInterval(period), queue: queue);
        }

        public static void Invoke(string pluginName, string assembly, string job, object[] paramers)
        {
            DomainInvoke<bool>(pluginName, assembly, job, paramers, PrefabricationActivator.Dispatch, domain => true);
        }


        public static void Test(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create<JobInvoke>(c => TestInvoke(pluginName, assembly, job, paramers), state);
        }

        public static bool TestInvoke(string pluginName, string assembly, string job, object[] paramers)
        {
            return DomainInvoke<bool>(pluginName, assembly, job, paramers, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }


        static T DomainInvoke<T>(string pluginName, string assembly, string job, object[] paramers, Action act, Func<AppDomain, T> GetResult)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, JobDomainManager.ServerName);
                var path = $"{ server.PlugPath }//{ pluginName }";
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(path),
                    ConfigurationFile = $"{path}\\App.config",
                    PrivateBinPath = path,
                    DisallowApplicationBaseProbing = false,
                    DisallowBindingRedirects = false
                };
                Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
                if (Directory.Exists(path) == false) throw (new Exception("此服务器不支持该插件"));
                var args = new CrossDomainData { PluginDir = path, assemblyName = assembly, typeName = job, paramers = paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(act));
                return GetResult(Domain);
            }
            finally
            {
                if (Domain != null) AppDomain.Unload(Domain);
            }
        }
    }


}

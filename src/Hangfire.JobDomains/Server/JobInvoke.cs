using Hangfire.JobDomains.Interface;
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

        public static void ScheduleInvoke(TimeSpan delay, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => ImmediatelyEnqueued(queue, pluginName, assembly, job, paramers), delay);
        }

        static void ImmediatelyEnqueued(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create(() => _JobInvoke.Invoke(pluginName, assembly, job, paramers), state);
        }

        public static void RecurringInvoke(string period, string queue, string jobSign, string pluginName, string assembly, string job, object[] paramers)
        {
            RecurringJob.AddOrUpdate(jobSign, () => _JobInvoke.Invoke(pluginName, assembly, job, paramers), period, queue: queue);
        }

        public static void Test(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create(() => _JobInvoke.TestInvoke(pluginName, assembly, job, paramers), state);
        }

    }

    internal class _JobInvoke
    {

        public static bool TestInvoke(string pluginName, string assembly, string job, object[] paramers)
        {
            return Invoke<bool>(pluginName, assembly, job, paramers, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }

        public static void Invoke(string pluginName, string assembly, string job, object[] paramers)
        {
            Invoke<bool>(pluginName, assembly, job, paramers, PrefabricationActivator.Dispatch, domain => true);
        }

        static T Invoke<T>(string pluginName, string assembly, string job, object[] paramers, Action act, Func<AppDomain, T> GetResult)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, PluginServiceManager.ServerName);
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

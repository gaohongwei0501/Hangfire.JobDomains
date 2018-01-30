using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Server;
using Hangfire.States;
using System;
using System.IO;

namespace Hangfire.JobDomains.Storage
{

    public class DynamicBaseService
    {

        public void Enqueued(JobParamer paramer)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
            hangFireClient.Create(() => Invoke(paramer), state);
        }

        public bool TestInvoke(JobParamer paramer)
        {
            return Invoke<bool>(paramer, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }

        public void Invoke(JobParamer paramer)
        {
            Invoke<bool>(paramer, PrefabricationActivator.Dispatch, domain => true);
        }

        static T Invoke<T>(JobParamer paramer, Action act, Func<AppDomain, T> GetResult)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, PluginServiceManager.ServerName);
                var path = $"{ server.PlugPath }//{ paramer.PluginName }";
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
                var args = new CrossDomainData { PluginDir = path, assemblyName = paramer.AssemblyFullName, typeName = paramer.JobName, paramers = paramer.JobParamers };
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



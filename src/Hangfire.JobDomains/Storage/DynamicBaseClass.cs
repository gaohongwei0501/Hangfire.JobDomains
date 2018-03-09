using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.States;
using System;
using System.IO;

namespace Hangfire.PluginPackets.Storage
{

    public class DynamicBaseClass
    {

        public void Enqueued(PluginParamer paramer)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
            hangFireClient.Create(() => Execute(paramer), state);
        }

        public bool Test(PluginParamer paramer)
        {
            return Execute<bool>(paramer, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }

        public void Execute(PluginParamer paramer)
        {
            Execute<bool>(paramer, PrefabricationActivator.Dispatch, domain => true);
        }

        static T Execute<T>(PluginParamer paramer, Action act, Func<AppDomain, T> GetResult)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, Environment.MachineName.ToLower());
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
                var args = new AssemblyParamerArg { PluginDir = path, AssemblyName = paramer.AssemblyFullName, TypeName = paramer.JobFullName, Paramers = paramer.JobParamers };
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



using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Storage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    /// <summary>
    /// 插件域任务执行
    /// </summary>
    public  class DomainJobExecute: JobExecute
    {
        protected override T Execute<T>(PluginParamer paramer, Action act, Func<AppDomain, T> GetResult)
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

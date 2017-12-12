using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Loader;
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
        public static void Invoke(string path, string assembly, string job, object[] paramers)
        {
            using (PluginHost host = new PluginHost(path))
            {
                if (host.LoadPlugins())
                {
                    Sponsor<object> objectFromPlugin = host.GetInstance(assembly, job, paramers);
                    if (objectFromPlugin == null) throw (new Exception("任务类构造函数的参数未正确提供不能实例化，或任务类未继承 MarshalByRefObject 而不能被发现"));
                    IPrefabrication instance = objectFromPlugin.Instance as IPrefabrication;
                    instance.Dispatch();
                }
                else
                {
                    throw (new Exception("no load"));
                }
            }
        }

        public static bool Test(string PluginPath, string assemblyName, string typeName, object[] paramers)
        {
            AppDomainSetup setup = new AppDomainSetup();
            var path = PluginPath; // GetAssemblyPath();
            setup.ApplicationBase = Path.GetDirectoryName(path);
            setup.ConfigurationFile = $"{path}\\App.config";
            setup.PrivateBinPath = PluginPath;
            setup.DisallowApplicationBaseProbing = false;
            setup.DisallowBindingRedirects = false;
            var Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
            try
            {
                var args = new CrossDomainData {  PluginDir= PluginPath, assemblyName= assemblyName, typeName= typeName, paramers= paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(TestInvoke.Invoke));
                return (bool)Domain.GetData("result");
            }
            finally {
                AppDomain.Unload(Domain);
            }
        }

    }
}

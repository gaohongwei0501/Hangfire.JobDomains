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
        public static void Invoke(string path, string assemblyName, string typeName, object[] paramers)
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

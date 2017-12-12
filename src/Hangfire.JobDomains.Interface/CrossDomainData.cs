using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Interface
{
    public static class TestInvoke
    {
        public static void Invoke()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            CrossDomainData args = domain.GetData("args") as CrossDomainData;
            Init(domain, args.PluginDir);
            var instance = domain.CreateInstance(args.assemblyName, args.typeName);
            var job = instance as IPrefabrication;
            domain.SetData("result", job.Test());
        }

        /// <summary>
        /// Loads plugin assemblies into the application domain and populates the collection of plugins.
        /// </summary>
        /// <param name="pluginDir"></param>
        /// <param name="disabledPlugins"></param>
        static void Init(AppDomain domain, string PluginDir)
        {
            if (Directory.Exists(PluginDir) == false) return;
            foreach (string dllFile in Directory.GetFiles(PluginDir, "*.dll"))
            {
                try
                {
                    Assembly asm = domain.Load(dllFile);
                }
                catch (ReflectionTypeLoadException)
                {

                }
                catch (BadImageFormatException)
                {
                }
                catch (Exception)
                {

                }
            }
        }
    }
    public class CrossDomainData : MarshalByRefObject
    {
        public string PluginDir;
        public string assemblyName;
        public string typeName;
        public object[] paramers;
    }
}

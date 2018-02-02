using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{

    public class PrefabricationActivator
    {
        public static void Test()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            CrossDomainData args = domain.GetData("args") as CrossDomainData;
            Init(domain, args.PluginDir);
            var instance = domain.CreateInstance(args.assemblyName, args.typeName) as ObjectHandle;
            var job = instance.Unwrap() as IPrefabrication;
            domain.SetData("result", job.Test());
        }

        public static void Dispatch()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            CrossDomainData args = domain.GetData("args") as CrossDomainData;
            Init(domain, args.PluginDir);
            var instance = domain.CreateInstance(args.assemblyName, args.typeName) as ObjectHandle;
            var job = instance.Unwrap() as IPrefabrication;
            job.Dispatch();
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
}

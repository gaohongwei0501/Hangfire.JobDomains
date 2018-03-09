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

    public static class BatchImportService
    {

        public static void FetchPeriodBatch()
        {
            AppDomain domain = AppDomain.CurrentDomain;
            AssemblyParamerArg args = domain.GetData("args") as AssemblyParamerArg;
            Init(domain, args.PluginDir);
            var instance = domain.CreateInstance(args.AssemblyName, args.TypeName) as ObjectHandle;
            var import = instance.Unwrap() as IBatchImport;
            var batches = import.GetPeriodBatch();
            var paramers = PeriodBatchConvertToPluginParamer(batches);
            domain.SetData("paramers", paramers);
        }

        static List<PluginParamer> PeriodBatchConvertToPluginParamer(List<IPeriodBatch> batches)
        {
            var list = new List<PluginParamer>();
            foreach (var batch in batches)
            {
                var type = batch.GetType();
                if (type.GenericTypeArguments.Length == 0) continue;
                var T_Arg_Type = type.GenericTypeArguments[0];
                var paramer = new PluginParamer
                {
                    QueueName = batch.BatchQueue,
                    PluginName =string.Empty,
                    AssemblyFullName = T_Arg_Type.Assembly.FullName,
                    AssemblyName = T_Arg_Type.Assembly.ManifestModule.Name.Replace(".dll", string.Empty),
                    JobFullName = T_Arg_Type.FullName,
                    JobName = T_Arg_Type.Name,
                    JobParamers =  batch.JobParamers,
                    JobPeriod = batch.BatchCron,
                    JobTitle =  batch.BatchTitle,
                };
                list.Add(paramer);
            }
            return list;
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

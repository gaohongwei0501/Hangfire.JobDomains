using Common.Logging;
using Hangfire.PluginPackets.Dynamic;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace Hangfire.PluginPackets.Command
{
    public class PluginPlanImportCommand
    {
        static string ServerName => Environment.MachineName.ToLower();

        static ILog loger = LogManager.GetLogger<PluginPlanImportCommand>();

        public static async Task CreatePlan()
        {
            var batchParamers = GetPluginsBatches();
            foreach (var paramer in batchParamers)
            {
                await TryInvoke(() => PluginJobCreateCommand.Schedule(paramer));
            }
        }

        public static async Task CreateExcute()
        {
            var batchParamers = GetPluginsBatches();
            foreach (var paramer in batchParamers)
            {
               await TryInvoke(() => TypeFactory.CreateInheritType<DomainJobExecute>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle));
            }
        }
        
        public static async Task ScanPluginPlan(string pluginBasePath)
        {
            if (string.IsNullOrEmpty(pluginBasePath)) return;
            if (Directory.Exists(pluginBasePath) == false) return;
            var paths = Directory.GetDirectories(pluginBasePath);

            foreach (var path in paths)
            {
                var index = path.LastIndexOf("\\");
                var dir = path.Substring(index + 1);
                var files = Directory.GetFiles($"{pluginBasePath}\\{dir}", "*.dll");
                var assemblies = new List<AssemblyDefine>();
                foreach (var assemblyFile in files)
                {
                    try
                    {
                        var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                        await ReadBatchImportAssembly(dir, assemblyItem);
                    }
                    catch (FileLoadException)
                    {
                        //默认公用程序集不需要反射
                    }
                }
            }
        }

        static List<PluginParamer> GetPluginsBatches()
        {
            var list = new List<PluginParamer>();
            var queues = StorageService.Provider.GetQueues(null, ServerName);
            var models = StorageService.Provider.GetQueuePlans(queues.Select(s => s.Name));
            foreach (var model in models)
            {
                var index = model.AssemblyName.IndexOf(',');
                var AssemblyName = model.AssemblyName.Substring(0, index);
                index = model.TypeName.LastIndexOf('.');
                var TypeName = model.TypeName.Substring(index+1);
                var one = new PluginParamer
                {
                    QueueName = model.QueueName,
                    PluginName = model.PlugName,
                    AssemblyFullName = model.AssemblyName,
                    AssemblyName = AssemblyName,
                    JobFullName = model.TypeName,
                    JobName = TypeName,
                    JobParamers = model.Args,
                    JobPeriod = model.Period,
                    JobTitle = model.PlanName,
                };
                list.Add(one);
            }
            return list;
        }

        static async Task ReadBatchImportAssembly(string path, Assembly assembly)
        {
            var types = GetInterfaceTypes<IBatchImport>(assembly);

            foreach (var type in types)
            {
                var arg = new AssemblyParamerArg
                {
                    PluginDir = path,
                    AssemblyName = assembly.FullName,
                    TypeName = type.FullName
                };

                var models = await PluginPlanImportCommand.ReadDefine(arg);
                await StorageService.Provider.AddQueuePlans(models);
            }
        }

        static async Task<List<QueuePlanDefine>>  ReadDefine(AssemblyParamerArg batchArg)
        {
            var list = new List<QueuePlanDefine>();
            var subs = FetchBatches(batchArg, BatchImportService.FetchPeriodBatch);
            foreach (var one in subs)
            {
                await TryInvoke(() =>
                {

                    var model = new QueuePlanDefine
                    {
                        QueueName = one.QueueName,
                        PlanName = one.JobTitle,
                        Args = one.JobParamers,
                        Period = one.JobPeriod,
                        PlugName = batchArg.PluginDir,
                        AssemblyName = one.AssemblyFullName,
                        TypeName = one.JobFullName,
                    };

                    if (string.IsNullOrEmpty(one.QueueName))
                    {
                        var queue = StorageService.Provider.GetSelfQueue(ServerName);
                        model.QueueName = queue.Name;
                    }

                    list.Add(model);
                });
            }
            return list;
        }

        static IEnumerable<Type> GetInterfaceTypes<T>(Assembly assembly)
        {
            var interfaceType = typeof(T);
            var findedTypes = new List<Type>();
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (var type in types.Where(t => t != null))
            {
                if (type.IsClass && !type.IsAbstract
                    && type.GetInterfaces().Any(@interface => @interface.GUID == interfaceType.GUID)
                   )
                {
                    findedTypes.Add(type);
                }
            }
            return findedTypes;
        }

        static List<PluginParamer> FetchBatches(AssemblyParamerArg args, Action CrossAct)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, ServerName);
                var path = $"{ server.PlugPath }//{ args.PluginDir }";
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
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(CrossAct));
                var list = new List<PluginParamer>();
                list.AddRange(Domain.GetData("paramers") as List<PluginParamer>);
                return list;
            }
            finally
            {
                if (Domain != null) AppDomain.Unload(Domain);
            }
        }

        /// <summary>
        /// 容错执行
        /// </summary>
        static Task TryInvoke(Action invoke)
        {
            try
            {
                return Task.Run(() => invoke());
            }
            catch (Exception ex)
            {
                loger.Error(ex);
                return Task.FromException(ex);
            }
        }

      
        /// <summary>
        /// 容错执行
        /// </summary>
        static Task TryInvoke(Func<Task> invoke)
        {
            try
            {
                return invoke();
            }
            catch (Exception ex)
            {
                loger.Error(ex);
                return Task.FromException(ex);
            }
        }
    }
}

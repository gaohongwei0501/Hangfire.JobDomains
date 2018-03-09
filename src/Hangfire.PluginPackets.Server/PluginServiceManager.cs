using Common.Logging;
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

namespace Hangfire.PluginPackets.Server
{
    internal class PluginServiceManager
    {

        public static string ServerName => Environment.MachineName.ToLower();

        static ILog loger = LogManager.GetLogger<PluginServiceManager>();

        public static async Task<BackgroundJobServerOptions> InitServer(string path, int workerCount)
        {
            if (string.IsNullOrEmpty(path))
            {
                var server = StorageService.Provider.GetServer(ServerName);
                if (server != null) path = server.PlugPath;
            }

            await UpdateServer(path);

            var queues = StorageService.Provider.GetQueues(null, ServerName);
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = workerCount,
                Queues = queues.Select(s => s.Name).ToArray()
            };
            return options;
        }

        public static async Task Restart(string path)
        {
            await UpdateServer(path);
            //暂时只支持 web 站重启
            File.SetLastWriteTime($"{AppDomain.CurrentDomain.BaseDirectory}\\Web.config", System.DateTime.Now);
        }

        public static async Task UpdateServer(string path)
        {
            try
            {
                var plugins = await ScanServer(path);
                var server = new ServerDefine(ServerName) { PlugPath = path };
                var success = await StorageService.Provider.AddOrUpdateServerAsync(server, plugins);
                if (success) return;
            }
            catch (Exception ex)
            {
                loger.Error("服务初始化", ex);
            }
            throw (new Exception(" 服务更新失败! "));
        }

        static async Task<List<string>> ScanServer(string basePath)
        {
            var plugins = new List<string>();
            if (string.IsNullOrEmpty(basePath)) return plugins;
            if (Directory.Exists(basePath) == false) Directory.CreateDirectory(basePath);
            var paths = Directory.GetDirectories(basePath);

            foreach (var path in paths)
            {
                var index = path.LastIndexOf("\\");
                var dir = path.Substring(index + 1);
                var define = new PluginDefine(dir);
                LoadPlugin(basePath, define);

                var sets = define.GetJobSets();
                if (sets == null || sets.Count == 0) continue;

                await StorageService.Provider.AddPluginAsync(define);
                plugins.Add(define.Title);
            }

            return plugins;
        }


        static void LoadPlugin(string basePath, PluginDefine plugin)
        {
            var files = Directory.GetFiles($"{basePath}\\{plugin.PathName}", "*.dll");
            var assemblies = new List<AssemblyDefine>();
            foreach (var assemblyFile in files)
            {
                try
                {
                    var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                    var assemblyDefine = CreateAssemblyDefine(plugin, assemblyItem);
                    var jobs = ReadPrefabricationAssembly(plugin, assemblyDefine, assemblyItem);
                    if (jobs.Count == 0) continue;

                    assemblyDefine.SetJobs(jobs);
                    assemblies.Add(assemblyDefine);

                    ReadBatchImportAssembly(plugin, assemblyDefine, assemblyItem);
                }
                catch (FileLoadException)
                {
                    //默认公用程序集不需要反射
                }
            }
            if (assemblies.Count == 0) return;
            plugin.SetJobSets(assemblies);
        }

        static AssemblyDefine CreateAssemblyDefine(PluginDefine domainDefine, Assembly define)
        {
            var file = define.Location;
            var fullName = define.FullName;
            var shortName = define.ManifestModule.Name.Replace(".dll", string.Empty);
            var title = define.ReadReflectionOnlyAssemblyAttribute<AssemblyTitleAttribute>();
            var description = define.ReadReflectionOnlyAssemblyAttribute<AssemblyDescriptionAttribute>();
            return new AssemblyDefine(domainDefine, file, fullName, shortName, title, description);
        }

        static List<JobDefine> ReadPrefabricationAssembly(PluginDefine plugin, AssemblyDefine assemblyDefine, Assembly assembly)
        {
            var list = new List<JobDefine>();
            var types = assembly.GetInterfaceTypes<IPrefabrication>();
            foreach (var type in types)
            {
                var (Title, Description) = type.ReadReflectionNameplateAttribute();
                var constructors = GetConstructors(type);
                var define = new JobDefine(assemblyDefine, type.FullName, type.Name, constructors, Title, Description);
                list.Add(define);
            }
            return list;
        }

        static IEnumerable<ConstructorDefine> GetConstructors(Type type)
        {
            var constructors = new List<ConstructorDefine>();
            var ctors = type.GetConstructors();
            foreach (var item in ctors)
            {
                var paramers = item.GetParameters();
                var one = new ConstructorDefine();
                foreach (var par in paramers)
                {
                    one.Paramers.Add((par.Name, par.ParameterType.Name));
                }
                constructors.Add(one);
            }
            return constructors;
        }

        public static void LoadDynamic()
        {
            var plugins = StorageService.Provider.GetPluginDefines();
            foreach (var plugin in plugins)
            {
                var sets = plugin.GetJobSets();
                foreach (var ass in sets)
                {
                    var jobs = ass.GetJobs();
                    foreach (var job in jobs)
                    {
                        DynamicFactory.Create<DynamicBaseClass>(plugin.PathName, ass.ShortName, job.Name, job.Title);
                    }
                }
            }

            AppDomain.CurrentDomain.AppendPrivatePath(DynamicFactory.DynamicPath);

        }

        public static void ImportPluginsBatch()
        {
            var batchParamers =  GetPluginsBatches();
            foreach (var paramer in batchParamers)
            {
                var service = new DynamicService(paramer);
                service.PeriodDispatch();
            }
        }

        static List<AssemblyParamerArg> BatchAssemblies { get; set; } = new List<AssemblyParamerArg>();

        static void ReadBatchImportAssembly(PluginDefine plugin, AssemblyDefine assemblyDefine, Assembly assembly)
        {
            var types = assembly.GetInterfaceTypes<IBatchImport>();
            foreach (var type in types)
            {
                var data = new AssemblyParamerArg
                {
                    PluginDir = plugin.PathName,
                    AssemblyName = assemblyDefine.FullName,
                    TypeName = type.FullName
                };
                BatchAssemblies.Add(data);
            }
        }

        static List<PluginParamer> GetPluginsBatches()
        {
            var list = new List<PluginParamer>();
            foreach (var batchArg in BatchAssemblies)
            {
                var subs = FetchBatches(batchArg, BatchImportService.FetchPeriodBatch);
                foreach (var one in subs) {
                    if (string.IsNullOrEmpty(one.QueueName)) {
                        var queue= StorageService.Provider.GetSelfQueue(ServerName);
                        one.QueueName = queue.Name;
                    }
                    one.PluginName = batchArg.PluginDir;
                    list.Add(one);
                }
            }
            return list;
        }

        static List<PluginParamer> FetchBatches(AssemblyParamerArg args,Action CrossAct)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, ServerName);
                var path = $"{ server.PlugPath }//{ args.PluginDir }";
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(path),
                    PrivateBinPath = path,
                    DisallowApplicationBaseProbing = false,
                    DisallowBindingRedirects = false
                };
                Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
                if (Directory.Exists(path) == false) throw (new Exception("此服务器不支持该插件"));
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(CrossAct ));
                var list = new List<PluginParamer>();
                list.AddRange(Domain.GetData("paramers") as List<PluginParamer>);
                return list;
            }
            finally
            {
                if (Domain != null) AppDomain.Unload(Domain);
            }
        }

    }

}

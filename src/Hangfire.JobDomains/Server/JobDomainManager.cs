using Hangfire.JobDomains.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Storage;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Server
{
    internal class JobDomainManager
    {

        public static async Task<BackgroundJobServerOptions> InitServer(string path, int workerCount)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var queues = await ScanServer(path);
            var success = await UpdateServer(path, queues);
            if (success == false) throw (new Exception(" 服务初始化失败! "));
            var options = CreateServerOptions(queues, workerCount);
            return options;
        }
     
        static async Task<bool> UpdateServer(string path, List<string> queues)
        {
            try
            {
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
                var server = new ServerDefine() { PlugPath= path };
                return await StorageService.Provider.AddOrUpdateServerAsync(server, queues);
            }
            catch
            {
                return false;
            }
        }

        static async Task<List<string>> ScanServer(string basePath)
        {
            var queues = new List<string>();
            var paths = Directory.GetDirectories(basePath);
            foreach (var path in paths)
            {
                var index = path.LastIndexOf("\\");
                var dir = path.Substring(index + 1);
                var define = new DomainDefine(dir);
                LoadDomain(basePath, define);
                await StorageService.Provider.AddDomainAsync(define);
                queues.Add(define.Title);
            }
            return queues;
        }
       
        static BackgroundJobServerOptions CreateServerOptions(List<string> queues, int workerCount)
        {
            var Queues = new List<string> { "default", Environment.MachineName.ToLower() };
            Queues.AddRange(queues);
            var options = new BackgroundJobServerOptions
            {
                WorkerCount = workerCount,
                Queues = Queues.ToArray()
            };
            return options;
        }

        static void LoadDomain(string basePath, DomainDefine define)
        {
            var files = Directory.GetFiles($"{basePath}\\{define.PathName}", "*.dll");
            var assemblies = new List<AssemblyDefine>();
            foreach (var assemblyFile in files)
            {
                try
                {
                    var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                    var assemblyDefine = CreateAssemblyDefine(define, assemblyItem);
                    var jobs = ReadPrefabricationAssembly(assemblyDefine, assemblyItem);
                    if (jobs.Count == 0) continue;
                    assemblyDefine.SetJobs(jobs);
                    assemblies.Add(assemblyDefine);
                }
                catch (FileLoadException)
                {
                    //默认公用程序集不需要反射
                }
            }
            if (assemblies.Count == 0) return;
            define.SetJobSets(assemblies);
        }

        static AssemblyDefine CreateAssemblyDefine(DomainDefine domainDefine, Assembly define)
        {
            var file = define.Location;
            var fullName = define.FullName;
            var shortName = define.ManifestModule.Name.Replace(".dll", string.Empty);
            var title = define.ReadReflectionOnlyAssemblyAttribute<AssemblyTitleAttribute>();
            var description = define.ReadReflectionOnlyAssemblyAttribute<AssemblyDescriptionAttribute>();
            return new AssemblyDefine(domainDefine, file, fullName, shortName, title, description);
        }

        static List<JobDefine> ReadPrefabricationAssembly(AssemblyDefine assemblyDefine, Assembly assembly)
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

    }

     
  

}

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

        public static async Task<BackgroundJobServerOptions>  InitServer(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var queues=await InitStorage(path);
            var options = CreateServerOptions(queues);
            await StorageService.Provider.UpdateServerDomains(Environment.MachineName.ToLower(), queues);
            return options;
        }
     
        public static async Task<bool> ChangePath(string path)
        {
            try
            {
                if (StorageService.Provider.IsDomainsEmpty == false) return false;
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
                var server = new ServerDefine() { PlugPath= path };
                return await StorageService.Provider.AddOrUpdateServerAsync(server);
            }
            catch
            {
                return false;
            }
        }

        static async Task<List<string>> InitStorage(string basePath)
        {
            var queues = new List<string>();
            var success =await ChangePath(basePath);
            if (success == false) return queues;
            var paths = Directory.GetDirectories(basePath);
            foreach (var path in paths)
            {
                var define = new DomainDefine(path);
                LoadDomain(define);
                await StorageService.Provider.AddDomainAsync(define);
                queues.Add(define.Name);
            }
            return queues;
        }

        static BackgroundJobServerOptions CreateServerOptions(List<string> queues) {
            var Queues = new List<string> { "default", Environment.MachineName.ToLower() };
            Queues.AddRange(queues);
            var options = new BackgroundJobServerOptions { Queues = Queues.ToArray() };
            return options;
        }

        static void LoadDomain(DomainDefine define)
        {
            var files = Directory.GetFiles(define.BasePath, "*.dll");
            var assemblies = new List<AssemblyDefine>();
            foreach (var assemblyFile in files)
            {
                var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                var assemblyDefine = CreateAssemblyDefine(define, assemblyItem);
                var jobs = ReadPrefabricationAssembly(assemblyDefine, assemblyItem);
                if (jobs.Count == 0) continue;
                assemblyDefine.SetJobs(jobs);
                assemblies.Add(assemblyDefine);
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
                var attr = type.ReadReflectionNameplateAttribute();
                var constructors = GetConstructors(type);
                var define = new JobDefine(assemblyDefine, type.FullName, type.Name, constructors, attr);
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

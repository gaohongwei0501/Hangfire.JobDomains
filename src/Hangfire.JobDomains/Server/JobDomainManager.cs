using Hangfire.JobDomains.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Storage;

namespace Hangfire.JobDomains.Server
{
    internal class JobDomainManager
    {

        public static BackgroundJobServerOptions InitServer(string path)
        {
            if (string.IsNullOrEmpty(path)) return null;
            var queues= InitStorage(path);
            var options = CreateServerOptions(queues);
            StorageService.Provider.UpdateServerDomains(Environment.MachineName.ToLower(), queues);
            return options;
        }
     
        public static bool ChangePath(string path)
        {
            try
            {
                if (StorageService.Provider.IsDomainsEmpty == false) return false;
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
                var server = new ServerDefine() { PlugPath= path };
                return StorageService.Provider.AddOrUpdateServer(server);
            }
            catch
            {
                return false;
            }
        }

        static List<string> InitStorage(string basePath)
        {
            var queues = new List<string>();
            var success = ChangePath(basePath);
            if (success == false) return queues;
            var paths = Directory.GetDirectories(basePath);
            foreach (var path in paths)
            {
                var files = Directory.GetFiles(path, "*.dll");
                var assemblies = new List<AssemblyDefine>();
                foreach (var assemblyFile in files)
                {
                    var assemblyItem = Assembly.ReflectionOnlyLoadFrom(assemblyFile);
                    var jobs = ReadPrefabricationAssembly(assemblyItem);
                    if (jobs.Count == 0) continue;
                    var assemblyDefine = CreateAssemblyDefine(assemblyItem, jobs);
                    assemblies.Add(assemblyDefine);
                }
                var define = new DomainDefine(path, assemblies);
                StorageService.Provider.Add(define);
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

        static AssemblyDefine CreateAssemblyDefine(Assembly define, List<JobDefine> jobs)
        {
            var file = define.Location;
            var fullName = define.FullName;
            var shortName = define.ManifestModule.Name.Replace(".dll", string.Empty);
            var title = define.ReadReflectionOnlyAssemblyAttribute<AssemblyTitleAttribute>();
            var description = define.ReadReflectionOnlyAssemblyAttribute<AssemblyDescriptionAttribute>();
            return new AssemblyDefine(file, fullName, shortName, title, description, jobs);
        }

        static List<JobDefine> ReadPrefabricationAssembly(Assembly assembly)
        {
            var list = new List<JobDefine>();
            var types = assembly.GetInterfaceTypes<IPrefabrication>();
            foreach (var type in types)
            {
                var attr = type.ReadReflectionOnlyTypeAttribute<NameplateAttribute>();
                var constructors = GetConstructors(type);
                var define = new JobDefine(type.FullName, type.Name, constructors, attr);
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
                foreach (var par in paramers) {
                    one.Paramers.Add((par.Name, par.ParameterType.Name));
                }
                constructors.Add(one);
            }
            return constructors;
        }

    }

     
  

}

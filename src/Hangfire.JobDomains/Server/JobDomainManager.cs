using Hangfire.Dashboard;
using Hangfire.JobDomains.Dashboard;
using Hangfire.JobDomains.Dashboard.Pages;
using Hangfire.JobDomains.Models;
using System;
using System.Runtime.Caching;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Security.Policy;
using Hangfire.JobDomains.Loader;
using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.AppSetting;
using Hangfire.JobDomains.Dashboard.Dispatchers;
using Hangfire.JobDomains.Storage;

namespace Hangfire.JobDomains.Server
{
    internal class JobDomainManager
    {

        public static void InitServer(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            InitStorage(path);
        }

     
        public static bool ChangePath(string path)
        {
            try
            {
                if (StorageService.Provider.IsEmpty == false) return false;
                if (Directory.Exists(path) == false) Directory.CreateDirectory(path);
                SysSetting.Dictionary.SetValue(SysSettingKey.BasePath, path);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static void InitStorage(string basePath)
        {
            var success = ChangePath(basePath);
            if (success == false) return;
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
                    var assemblyDefine =  CreateAssemblyDefine(assemblyItem, jobs);
                    assemblies.Add(assemblyDefine);
                }
                var define = new DomainDefine(path, assemblies);
                StorageService.Provider.Add(path, define);
            }
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

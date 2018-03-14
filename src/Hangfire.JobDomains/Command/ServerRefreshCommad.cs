using Common.Logging;
using Hangfire.PluginPackets._Helper;
using Hangfire.PluginPackets.Config;
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

    public class ServerRefreshCommad
    {

        static ILog loger = LogManager.GetLogger<ServerRefreshCommad>();

        static string ServerName => Environment.MachineName.ToLower();

        public static async Task Invoke(string path)
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
    

        //static ServiceSettings ServerSetting = new ServiceSettings(AppDomain.CurrentDomain.BaseDirectory, "Hangfire.PluginPackets.Service");

        //public static async Task<JsonData> Invoke(string path)
        //{
        //    var value = ServerSetting.GetValue();
        //    var result =await StopService(value.ServiceName);
        //    if (result.IsSuccess == false) return result;
        //    return await StartService(value.ServiceName);
        //}

        //async static Task<JsonData> StartService(string name)
        //{
        //    var result=  await SystemCommad.Exec($"net  start \"{ name }\"");
        //    return new JsonData {
        //          IsSuccess= result,
        //          Message= result ?"":"服务器正忙，请稍后尝试",
        //    };
        //}

        //async static Task<JsonData> StopService(string name)
        //{
        //    var result=  await SystemCommad.Exec($"net  stop \"{ name }\"");
        //    return new JsonData
        //    {
        //        IsSuccess = result,
        //        Message = result ? "" : "服务器正忙，请稍后尝试",
        //    };
        //}
    }
}

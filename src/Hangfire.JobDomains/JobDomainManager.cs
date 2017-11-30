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

namespace Hangfire.JobDomains
{
    internal class JobDomainManager
    {

        public static Dictionary<int, string> CronDictionary = new Dictionary<int, string>();

        public static void InitBoard(string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            InitStorage(path);
        }

        public static void InitRoute()
        {
            DashboardRoutes.Routes.Add("/jsex/domainJob", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.domainJob.js"));
            DashboardRoutes.Routes.Add("/cssex/jobdomain", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.JobDomains.css"));
            DashboardRoutes.Routes.Add("/image/loading.gif", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.image.loading.gif"));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.MainPageRoute, x => new MainPage());

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.SystemPageRoute, x => new SystemPage());

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.BatchSchedulePageRoute, x => new BatchSchedulePage());

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.DomainPageRoute, x => UrlHelperExtension.CreateDomainPage(x));

        //    DashboardRoutes.Routes.Add(UrlHelperExtension.DomainCommandPageRoute, x => UrlHelperExtension.CreateDomainCommandPage(x));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.AssemblyPageRoute, x => UrlHelperExtension.CreateAssemblyPage(x));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.JobPageRoute, x => UrlHelperExtension.CreateJobPage(x));

            DashboardRoutes.Routes.Add(UrlHelperExtension.JobCommandRoute,  new JobCommandDispatcher());

            NavigationMenu.Items.Add(page => new MenuItem(MainPage.Title, page.Url.To(UrlHelperExtension.MainPageRoute))
            {
                Active = page.RequestPath.StartsWith(UrlHelperExtension.MainPageRoute)
            });
        }

        public static bool ChangePath(string path)
        {
            try
            {
                if (DomainDefines.IsEmpty == false) return false;
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
                    var assemblyDefine = new AssemblyDefine(assemblyItem, jobs);
                    assemblies.Add(assemblyDefine);
                }
                var define = new DomainDefine(path, assemblies);
                DomainDefines.Add(path, define);
            }
        }

        public static List<DomainDefine> GetDomainDefines()
        {
            var list = new List<DomainDefine>();
            var path = SysSetting.Dictionary.GetValue<string>(SysSettingKey.BasePath);
            if (string.IsNullOrEmpty(path)) return list;
            return DomainDefines.GetAll();
        }

        static List<JobDefine> ReadPrefabricationAssembly(Assembly assembly)
        {
            var list = new List<JobDefine>();
            var types = assembly.GetInterfaceTypes<IPrefabrication>();
            foreach (var type in types)
            {
                var define = new JobDefine(type);  
                list.Add(define);
            }
            return list;
        }

        public static AppDomain GetDomain(DomainDefine define)
        {
            Func<Tuple<AppDomain, TimeSpan>> FetchValue = () =>
            {

                AppDomainSetup setup = new AppDomainSetup();
                setup.ApplicationBase = Path.GetDirectoryName(typeof(PluginHost).Assembly.Location);
                setup.PrivateBinPath = define.BasePath;
                setup.DisallowApplicationBaseProbing = false;
                setup.DisallowBindingRedirects = false;

                //var DynamicDomain = AppDomain.CreateDomain(define.Guid, null, define.BasePath, string.Empty, false);
                var DynamicDomain = AppDomain.CreateDomain(define.Name, null, setup);

                // instantiate PluginLoader in the other AppDomain
                var Loader = (PluginLoader)DynamicDomain.CreateInstanceAndUnwrap(
                    typeof(PluginLoader).Assembly.FullName,
                    typeof(PluginLoader).FullName
                );

                // since Sandbox was loaded from another AppDomain, we must sponsor 
                // it for as long as we need it
                var Sponsor = new Sponsor<PluginLoader>(Loader);

                Loader.Init(define.BasePath);

                // var loadeds = DynamicDomain.GetAssemblies();

                return new Tuple<AppDomain, TimeSpan>(DynamicDomain, new TimeSpan(0, 10, 0));
            };
            return DomainStorage.GetDomain(define.Name, FetchValue);
        }

    }


    internal class DomainStorage
    {
        static ObjectCache Storage = new MemoryCache("____DomainStorage____");

        public static AppDomain GetDomain(string guid, Func<Tuple<AppDomain, TimeSpan>> FetchValue)
        {
            return Get<AppDomain>(guid,FetchValue);
        }

        static T Get<T>(string key, Func<Tuple<T, TimeSpan>> FetchValue) where T : class
        {
            if (Storage.Contains(key)) return Storage.Get(key) as T;
            if (FetchValue == null) return default(T);
            var item = FetchValue();
            var value = item.Item1;
            var expires = item.Item2;
            AddOrUpdate(key, value, expires);
            return value;
        }

        /// <summary>
        /// 添加或更新缓存，默认采用固定时间过期策略
        /// </summary>
        static void AddOrUpdate(string key, object value, TimeSpan expire)
        {
            var item = new CacheItem(key) { Value = value };
            var policy = new CacheItemPolicy() { AbsoluteExpiration = DateTimeOffset.Now.Add(expire) };
            var isAdd = Storage.Add(item, policy);
            if (isAdd == false)
            {
                Storage.Set(item, policy);
            }
        }
    }


    /// <summary>
    /// 仓储
    /// </summary>
    internal class DomainDefines
    {

        static ConcurrentDictionary<string, DomainDefine> Storage = new ConcurrentDictionary<string, DomainDefine>();

        public static bool IsEmpty { get { return Storage.IsEmpty; } } 

        public static void Add(string key, DomainDefine define)
        {
            Storage.TryAdd(key, define);
        }

        public static List<DomainDefine> GetAll()
        {
            return Storage.Select(s=>s.Value).ToList();
        }
      

    }

  

}

using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using System.Collections.Concurrent;
using Hangfire;
using Microsoft.Owin;
using Hangfire.JobDomains.Server;
using Hangfire.JobDomains.Storage;
using Owin;
using Hangfire.JobDomains.Dashboard.Dispatchers;
using Hangfire.JobDomains.Dashboard;
using Hangfire.JobDomains.Dashboard.Pages;
using System.Net;

namespace Hangfire.JobDomains
{
    /// <summary>
    /// 运行模式
    /// </summary>
    public enum HangfireDomainMode
    {
        /// <summary>
        /// 服务器模式
        /// </summary>
        Server,
        /// <summary>
        /// 客户端模式
        /// </summary>
        Client,
        /// <summary>
        /// 共存模式
        /// </summary>
        All
    }

    public static class GlobalConfigurationExtension
    {

        /// <summary>
        /// 运行模式
        /// </summary>
        internal static HangfireDomainMode GlobalMode { get;  set; } = HangfireDomainMode.Client;

        /// <summary>
        /// 任务域服务（单机模式）
        /// </summary>
        public static async Task UseHangfirePlugins<T>(this IAppBuilder app, string path = "", string controllerName = "/HangfireDomain", string connectString = "", int workerCount = 5) where T : IDomainStorage, new()
        {
            GlobalMode = HangfireDomainMode.All;
            await app.InitPluginsAtServer<T>(path, connectString, workerCount);
            app.InitPluginsAtClient<T>(controllerName, connectString);
        }

        /// <summary>
        /// 任务域服务(服务器模式）
        /// </summary>
        public static async Task UseHangfirePluginsAtServer<T>(this IAppBuilder app, string path = "", string connectString = "", int workerCount = 5) where T : IDomainStorage, new()
        {
            GlobalMode = HangfireDomainMode.Server;
            await app.InitPluginsAtServer<T>(path, connectString, workerCount);
        }

        /// <summary>
        /// 任务域服务（客户端模式）
        /// </summary>
        public static void UseHangfirePluginsAtClient<T>(this IAppBuilder app, string controllerName = "/HangfireDomain", string connectString = "") where T : IDomainStorage, new()
        {
            GlobalMode = HangfireDomainMode.Client;
            app.InitPluginsAtClient<T>(controllerName, connectString);
            InitLocalRoute();
        }


        static async Task InitPluginsAtServer<T>(this IAppBuilder app, string path = "", string connectString = "", int workerCount = 5) where T : IDomainStorage, new()
        {
            var connecting = StorageService.Provider.SetStorage(new T(), connectString);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));
            var Options= await PluginServiceManager.InitServer(path, workerCount);

            PluginServiceManager.LoadDynamic();
            app.UseHangfireServer(Options);
        }

        static void InitPluginsAtClient<T>(this IAppBuilder app, string controllerName= "/HangfireDomain", string connectString = "") where T : IDomainStorage, new()
        {
            var connecting = StorageService.Provider.SetStorage(new T(), connectString);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));
            app.UseHangfireDashboard(controllerName);
            InitRoute();
            InitLocalRoute();
        }


        public static void InitRoute()
        {

            DashboardRoutes.Routes.Add("/jsex/domainScript", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.domainScript.js"));
            DashboardRoutes.Routes.Add("/cssex/domainStyle", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.domainStyle.css"));
            DashboardRoutes.Routes.Add("/image/loading.gif", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.JobDomains.Dashboard.Content.image.loading.gif"));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.MainPageRoute, x => new MainPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.SystemPageRoute, x => new SystemPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.BatchSchedulePageRoute, x => new BatchSchedulePage());

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.ServerListPageRoute, x => new  ServerListPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.ServerPageRoute, x => UrlHelperExtension.CreateServerPage(x));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.QueueListPageRoute, x => new QueueListPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.QueuePageRoute, x => UrlHelperExtension.CreateQueuePage(x));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.FolderPageRoute, x => UrlHelperExtension.CreateFolderPage(x));
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.AssemblyPageRoute, x => UrlHelperExtension.CreateAssemblyPage(x));
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.JobPageRoute, x => UrlHelperExtension.CreateJobPage(x));

            DashboardRoutes.Routes.Add(UrlHelperExtension.JobCommandRoute, new JobCommandDispatcher());
            DashboardRoutes.Routes.Add(UrlHelperExtension.ServerCommandRoute, new ServerCommandDispatcher());

            NavigationMenu.Items.Add(page => new MenuItem(MainPage.Title, page.Url.To(UrlHelperExtension.MainPageRoute))
            {
                Active = page.RequestPath.StartsWith(UrlHelperExtension.MainPageRoute)
            });
        }

        public static void InitLocalRoute()
        {
            if (GlobalMode == HangfireDomainMode.Client) return;

            var link = UrlHelperExtension.ServerPageRoute.Replace("(?<name>.+)", PluginServiceManager.ServerName);

            NavigationMenu.Items.Add(page => new MenuItem("本机服务", page.Url.To(link))
            {
                Active =  false
            });
        }

        static string GetLocalIp()
        {
            string hostname = Dns.GetHostName();//得到本机名   
            IPHostEntry localhost = Dns.GetHostEntry(hostname);
            IPAddress localaddr = localhost.AddressList[0];
            return localaddr.ToString();
        }

    }
}

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

namespace Hangfire.JobDomains
{

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
        /// 单机模式
        /// </summary>
        All
    }

    public static class GlobalConfigurationExtension
    {
        /// <summary>
        /// 
        /// </summary>
        public static HangfireDomainMode GlobalMode { get;private set; }

        /// <summary>
        /// 任务域服务（单机模式）
        /// </summary>
        /// <param name="config">全局配置</param>
        /// <param name="path">插件路径</param>
        public static void UseDomains<T>(this IAppBuilder app,  string path, string connectString="") where T : IDomainStorage, new()
        {
            if (string.IsNullOrEmpty(path)) return;
            StorageService.Storage = new T();
            var connecting=  StorageService.Storage.SetConnectString(connectString);
            if (connecting == false) throw (new  Exception(" HangfireDomain 数据服务连接失败"));

            app.UseHangfireServer();
            JobDomainManager.InitServer(path);

            app.UseHangfireDashboard();
            InitRoute();

        }

        /// <summary>
        /// 任务域服务(服务器模式）
        /// </summary>
        /// <param name="config"></param>
        /// <param name="path"></param>
        public static void UseDomainsAtServer<T>(this IAppBuilder app, string path, string connectString) where T: IDomainStorage,new()
        {
            if (string.IsNullOrEmpty(path)) return;
            StorageService.Storage = new T();
            var connecting = StorageService.Storage.SetConnectString(connectString);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));

            app.UseHangfireServer();
            JobDomainManager.InitServer(path);
        }

        /// <summary>
        /// 任务域服务（客户端模式）
        /// </summary>
        public static void UseDomainsAtClient<T>(this IAppBuilder app, string connectString) where T : IDomainStorage, new()
        {
            StorageService.Storage = new T();
            var connecting = StorageService.Storage.SetConnectString(connectString);
            if (connecting == false) throw (new Exception(" HangfireDomain 数据服务连接失败"));

            app.UseHangfireDashboard();
            InitRoute();
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
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.AssemblyPageRoute, x => UrlHelperExtension.CreateAssemblyPage(x));
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.JobPageRoute, x => UrlHelperExtension.CreateJobPage(x));
            DashboardRoutes.Routes.Add(UrlHelperExtension.JobCommandRoute, new JobCommandDispatcher());

            NavigationMenu.Items.Add(page => new MenuItem(MainPage.Title, page.Url.To(UrlHelperExtension.MainPageRoute))
            {
                Active = page.RequestPath.StartsWith(UrlHelperExtension.MainPageRoute)
            });
        }

    }

}

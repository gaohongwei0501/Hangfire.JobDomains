using System;
using System.Reflection;
using Hangfire.Dashboard;
using Hangfire.PluginPackets.Storage;
using Owin;
using Hangfire.PluginPackets.Dashboard.Dispatchers;
using Hangfire.PluginPackets.Dashboard.Pages;
using System.Net;
using Hangfire.PluginPackets.Dynamic;
using System.Threading;

namespace Hangfire.PluginPackets.Dashboard
{
   

    public static class OwinExtensions
    {

        /// <summary>
        /// 任务域服务（客户端模式）
        /// </summary>
        public static void UseHangfirePluginDashboard<T>(this IAppBuilder app, string controllerName = "/PluginPackets", string connectString = "") where T : IStorage, new()
        {
            var connecting = StorageService.Provider.SetStorage(new T(), connectString);
            if (connecting == false) throw (new Exception(" Hangfire.PluginPackets 数据服务连接失败"));

            app.UseHangfireDashboard(controllerName, new DashboardOptions()
            {
                Authorization = new[] { new CustomAuthorizeFilter() }
            });
            InitRoute();
        }

        static void InitRoute()
        {

            DashboardRoutes.Routes.Add("/jsex/domainScript", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.PluginPackets.Dashboard.Content.domainScript.js"));
            DashboardRoutes.Routes.Add("/cssex/domainStyle", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.PluginPackets.Dashboard.Content.domainStyle.css"));
            DashboardRoutes.Routes.Add("/image/loading.gif", new EmbeddedResourceDispatcher(Assembly.GetExecutingAssembly(), "Hangfire.PluginPackets.Dashboard.Content.image.loading.gif"));

            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.MainPageRoute, x => new MainPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.SystemPageRoute, x => new SystemPage());
            DashboardRoutes.Routes.AddRazorPage(UrlHelperExtension.ClientPageRoute, x => new ClientPage());
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
            DashboardRoutes.Routes.Add(UrlHelperExtension.ClientCommandRoute, new ClientCommandDispatcher());

            NavigationMenu.Items.Add(page => new MenuItem(MainPage.Title, page.Url.To(UrlHelperExtension.MainPageRoute))
            {
                Active = page.RequestPath.StartsWith(UrlHelperExtension.MainPageRoute)
            });

            NavigationMenu.Items.Add(page => new MenuItem(ClientPage.Title, page.Url.To(UrlHelperExtension.ClientPageRoute))
            {
                Active = page.RequestPath.StartsWith(UrlHelperExtension.ClientPageRoute)
            });
        }
       
    }
}

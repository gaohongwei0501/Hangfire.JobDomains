using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using System.Web;
using Owin;
using Hangfire.JobDomains;
using Hangfire.SQLite;
using Hangfire;
using System.Threading.Tasks;

[assembly: OwinStartup(typeof(Host.Startup))]
namespace Host
{
    public class Startup
    {

        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            GlobalConfiguration.Configuration.UseSQLiteStorage(@"Data Source=|DataDirectory|\Store.dat;Version=3;");
            GlobalConfiguration.Configuration.UseDomains(@"E:\Hangfile.Sparepart.Lib");
            app.UseHangfireServer();
            app.UseHangfireDashboard();

            app.Run(context =>
            {
                context.Response.Redirect("/Hangfire");
                return context.Response.WriteAsync("Hello, world.");
            });
        }

    }
}
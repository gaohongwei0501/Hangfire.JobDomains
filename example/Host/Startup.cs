using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using System.Web;
using Owin;
using Hangfire.PluginPackets;
using Hangfire.SQLite;
using Hangfire.MemoryStorage;
using Hangfire;
using System.Threading.Tasks;
using Hangfire.PluginPackets.Storage.EntityFrameworkCore;
using Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer;
using Hangfire.PluginPackets.Storage.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Hangfire.PluginPackets.Server;
using Hangfire.PluginPackets.Dashboard;

[assembly: OwinStartup(typeof(Host.Startup))]
namespace Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {

            //var filePath = @"E:\Hangfile.Sparepart.Lib"; 
            //GlobalConfiguration.Configuration. ();
            //var LoadAsyc = app.UseHangfirePlugins<Hangfire.JobDomains.Storage.EntityFrameworkCore.Memory.MemoryStorage>(filePath);

            var dataConnectString = "ConnectionString";
            GlobalConfiguration.Configuration.UseSqlServerStorage(dataConnectString);
            //var LoadAsyc = app.UseHangfirePluginServer<SqlServerStorage>(connectString: dataConnectString);
            //Task.WaitAll(LoadAsyc);
            app.UseHangfirePluginDashboard<SqlServerStorage>(connectString: dataConnectString);

            app.Run(context =>
            {
                context.Response.Redirect("/HangfireDomain");
                return context.Response.WriteAsync("Hello, world.");
            });

        }

    }


}
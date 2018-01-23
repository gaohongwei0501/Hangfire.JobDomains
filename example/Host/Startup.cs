using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Owin;
using System.Web;
using Owin;
using Hangfire.JobDomains;
using Hangfire.SQLite;
using Hangfire.MemoryStorage;
using Hangfire;
using System.Threading.Tasks;
using Hangfire.JobDomains.Storage.Location;
using Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer;
using Hangfire.JobDomains.Storage.EntityFrameworkCore.Sqlite;
using Microsoft.Extensions.DependencyInjection;

[assembly: OwinStartup(typeof(Host.Startup))]
namespace Host
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            // 有关如何配置应用程序的详细信息，请访问 http://go.microsoft.com/fwlink/?LinkID=316888
            var filePath = @"E:\Hangfile.Sparepart.Lib";
            var dataPath = @"Data Source=E:\Hangfile.Sparepart.Lib\Data\Store.dat;";//Version=3;

            //  GlobalConfiguration.Configuration.UseMemoryStorage();
            //  app.UseDomains<LocationStorage>(filePath);

            //  GlobalConfiguration.Configuration.UseSQLiteStorage(dataPath);
            //  app.UseDomains<Hangfire.JobDomains.Storage.Sqlite.SQLiteStorage>(filePath, dataPath);

            var dataConnectString = "ConnectionString";
            GlobalConfiguration.Configuration.UseSqlServerStorage(dataConnectString);

            //app.UseDomains<LocationStorage>(filePath, connectString);
            app.UseDomains<SqlServerStorage>(connectString: dataConnectString);

            app.Run(context =>
            {
                context.Response.Redirect("/HangfireDomain");
                return context.Response.WriteAsync("Hello, world.");
            });

        }

    }
}
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
using Hangfire.JobDomains.Storage.Location;

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
            GlobalConfiguration.Configuration.UseSQLiteStorage(dataPath);
         //   app.UseDomains<LocationStorage>(@"E:\Hangfile.Sparepart.Lib");
            app.UseDomains<Hangfire.JobDomains.Storage.Sqlite.SQLiteStorage>(filePath, dataPath);
            app.Run(context =>
            {
                context.Response.Redirect("/HangfireDomain");
                return context.Response.WriteAsync("Hello, world.");
            });
        }

    }
}
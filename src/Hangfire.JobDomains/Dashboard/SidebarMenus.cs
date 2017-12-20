using Hangfire.Annotations;
using Hangfire.Dashboard;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard
{

    internal class SidebarMenus
    {

        public static Func<List<Func<RazorPage, MenuItem>>> DefaultMenu = () =>
        {
            var menus = new List<Func<RazorPage, MenuItem>>();

            MenuItem CreatePageMenu(RazorPage page, (string Name, string Link) route) => new MenuItem(route.Name, route.Link)
            {
                Active = page.RequestPath.StartsWith(route.Link)
            };

            menus.Add(page => CreatePageMenu(page, page.Url.CreateRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateServerListRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateQueueListRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateSystemRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateBatchRoute()));

            return menus;
        };

        public static Func<RazorPage, string, List<Func<RazorPage, MenuItem>>> ServersMenu = (master, current) =>
         {
             var menus = new List<Func<RazorPage, MenuItem>>
             {
                page =>{
                      var (Name, Link) = page.Url.CreateServerListRoute();
                      return   new MenuItem(Name, Link);
                }
             };
 
             var servers = StorageService.Provider.GetServers(master.Storage).OrderBy(s => s.Name);

             foreach (var one in servers)
             {
                 menus.Add(page =>
                 {
                     var (Name, Link) = page.Url.CreateServerRoute(one);
                     return new MenuItem(Name, Link)
                     {
                         Active = one.Name == current
                     };
                 });
             }
             return menus;
         };

        public static Func<RazorPage,string, List<Func<RazorPage, MenuItem>>> QueuesMenu = (master,current) =>
        {

            var menus = new List<Func<RazorPage, MenuItem>>
             {
                 page =>{
                      var (Name, Link) = page.Url.CreateQueueListRoute();
                      return  new MenuItem(Name, Link);
                }
             };

            var queues = StorageService.Provider.GetQueues(master.Storage).OrderBy(s => s.Name);

            foreach (var one in queues)
            {
                menus.Add(page =>
                {
                    var (Name, Link) = page.Url.CreateQueueRoute(one);
                    return new MenuItem(Name, Link)
                    {
                        Active = one.Name == current
                    };
                });
            }
            return menus;
        };

        public static Func<string, List<Func<RazorPage, MenuItem>>> DomainsMenu = (current) =>
        {

            var menus = new List<Func<RazorPage, MenuItem>>
            {
                page => new MenuItem("任务包列表", "#")
            };

            var domains = StorageService.Provider.GetDomainDefines().OrderBy(s => s.Title);

            foreach (var one in domains)
            {
                menus.Add(page =>
                {
                    var (Name, Link) = page.Url.CreateRoute(one);
                    return new MenuItem(Name, Link)
                    {
                        Active = one.Title == current
                    };
                });
            }
            return menus;
        };


        public static Func<string, string, string, List<Func<RazorPage, MenuItem>>> JobsMenu = (d, a, j) =>
        {

            var menus = new List<Func<RazorPage, MenuItem>>();

            var set = StorageService.Provider.GetDomainDefines();
            var theDomain = set.SingleOrDefault(s => s.Title == d);
            if (theDomain == null) return menus;
            var theSet = theDomain.GetJobSets().SingleOrDefault(s => s.ShortName == a);
            if (theSet == null) return menus;

            menus.Add(page =>
            {
                var (Name, Link) = page.Url.CreateRoute(theDomain, theSet);
                return new MenuItem($"{ theSet.Title } 任务列表", Link)
                {
                    Active = false
                };
            });

            var jobs = theSet.GetJobs().OrderBy(s => s.Title);
            foreach (var one in jobs)
            {
                menus.Add(page =>
                {
                    var (Name, Link) = page.Url.CreateRoute(theDomain, theSet, one);
                    return new MenuItem(Name, Link)
                    {
                        Active = page.RequestPath.StartsWith(Link)
                    };
                });
            }
            return menus;
        };

    }
}

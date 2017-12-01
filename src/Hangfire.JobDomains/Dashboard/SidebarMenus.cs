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

            Func<RazorPage, (string Name, string Link), MenuItem> CreatePageMenu = (page, route) => new MenuItem(route.Name, route.Link)
            {
                Active = page.RequestPath.StartsWith(route.Link)
            };

            menus.Add(page => CreatePageMenu(page, page.Url.CreateRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateSystemRoute()));
            menus.Add(page => CreatePageMenu(page, page.Url.CreateBatchRoute()));
         
            return menus;
        };

        public static Func<string, List<Func<RazorPage, MenuItem>>> DomainsMenu = (current) =>
         {

             var menus = new List<Func<RazorPage, MenuItem>>();

             menus.Add(page => new MenuItem("任务包列表", "#"));

             var domains = StorageService.Provider.GetDomainDefines().OrderBy(s => s.Name);

             foreach (var one in domains)
             {
                 menus.Add(page =>
                 {
                     var oneRoute = page.Url.CreateRoute(one);
                     return new MenuItem(oneRoute.Name, oneRoute.Link)
                     {
                         Active = one.Name == current
                     };
                 });
             }
             return menus;
         };

        public static Func<string, string, string, List<Func<RazorPage, MenuItem>>> JobsMenu = (d, a, j) =>
        {

            var menus = new List<Func<RazorPage, MenuItem>>();

            var set = StorageService.Provider.GetDomainDefines();
            var theDomain = set.SingleOrDefault(s => s.Name == d);
            if (theDomain == null) return menus;
            var theSet = theDomain.JobSets.SingleOrDefault(s => s.ShortName == a);
            if (theSet == null) return menus;

            menus.Add(page =>
            {
                var route = page.Url.CreateRoute(theDomain, theSet);
                return new MenuItem($"{ theSet.Title } 任务列表", route.Link)
                {
                    Active = false
                };
            });

            var jobs = theSet.Jobs.OrderBy(s => s.Title);

            foreach (var one in jobs)
            {
                menus.Add(page =>
                {
                    var route = page.Url.CreateRoute(theDomain, theSet, one);
                    return new MenuItem(route.Name, route.Link)
                    {
                        Active = page.RequestPath.StartsWith(route.Link)
                    };
                });
            }
            return menus;
        };

    }
}

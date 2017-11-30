using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{

    internal class AssemblyPage : HtmlPage
    {
      
        public DomainDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public AssemblyPage(string domain, string name)
        {
            FetchTitle = () => $"{ (TheAssembly == null ? string.Empty : TheAssembly.ShortName) } 程序集任务定义详情";
            FetchHeader = () => TheAssembly.Title;

            Sidebar = ()=>SidebarMenus.DomainsMenu(domain);
            var set = JobDomainManager.GetDomainDefines();
            TheDomain = set.SingleOrDefault(s => s.Name == domain);
            TheAssembly = TheDomain == null ? null : TheDomain.JobSets.SingleOrDefault(s => s.ShortName == name);
        }

        protected override bool Content()
        {
            if (TheAssembly == null) return NoFound();
            return Nomal();
        }

        void WriteBar()
        {
            var mainRoute = Url.CreateRoute();
            var domainRoute = Url.CreateRoute(TheDomain);
            var bar = $@"<ol class='breadcrumb'>
                          <li><a href='{ mainRoute.Link }'>{ mainRoute.Name }</a></li>
                          <li><a href='{ domainRoute.Link }'>{ domainRoute.Name }</a></li>
                          <li class='active'><a href='#'>{ Url.CreateRoute(TheDomain, TheAssembly).Name }</a></li>
                        </ol>";
            WriteLiteral(bar);
        }

        bool NoFound()
        {
            WriteLiteral($@"<div class=""alert alert-danger"" role=""alert"">程序集未被发现</div>");
            return false;
        }

        bool Nomal()
        {
            WriteBar();
            PageContent.WritePagerPanel(TheAssembly.ShortName, TheAssembly.Jobs, PageIndex, PageSize, d => Url.CreateRoute(TheDomain, TheAssembly, d));
            return true;
        }

      

    }

}

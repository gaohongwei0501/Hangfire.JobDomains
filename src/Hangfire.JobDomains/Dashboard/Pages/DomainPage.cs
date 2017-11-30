using Hangfire.Dashboard;
using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{

    internal class DomainPage : HtmlPage
    {

        public const string Title = "程序集详情";

        public DomainDefine TheDomain { get; set; }

        public DomainPage(string name) 
        {
            FetchTitle = () => Title;
            FetchHeader = () => TheDomain == null ? name : TheDomain.Name;

            Sidebar = ()=>SidebarMenus.DomainsMenu(name);
            var set = JobDomainManager.GetDomainDefines();
            TheDomain = set.SingleOrDefault(s => s.Name == name);
        }

        protected override bool Content()
        {
            if (TheDomain == null) return NoFound();
            WriteBar();
            if (TheDomain.JobSets.Count == 0) return None();
            PageContent.WritePagerPanel($@"任务程序集", TheDomain.GetJobSets(), PageIndex, PageSize, d => Url.CreateRoute(TheDomain, d));
            return true;
        }

        bool NoFound()
        {
            WriteLiteral($@"<div class=""alert alert-danger"" role=""alert"">程序集未被发现</div>");
            return false;
        }

        bool None()
        {
            var panel= PageContent.Tag.Panel("任务程序集", "插件包中所有应用程序集都不存在任务类型的定义！", string.Empty, string.Empty);
            WriteLiteral(panel);
            return true;
        }

        void WriteBar()
        {
            var mainRoute = Url.CreateRoute();
            var bar = $@"<ol class='breadcrumb'>
                          <li><a href='{ mainRoute.Link }'>{ mainRoute.Name }</a></li>
                          <li class='active'><a href='#'>{ Url.CreateRoute(TheDomain).Name }</a></li>
                        </ol>";
            WriteLiteral(bar);
        }

       
        
    }
}

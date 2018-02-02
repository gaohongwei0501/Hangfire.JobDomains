using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{

    internal class AssemblyPage : HtmlPage
    {
      
        public PluginDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public AssemblyPage(string domain, string name)
        {
            FetchTitle = () => $"{ (TheAssembly == null ? string.Empty : TheAssembly.ShortName) } 程序集任务定义详情";
            FetchHeader = () => $"任务集：{(TheAssembly == null ? string.Empty : TheAssembly.Title)}" ;

            Sidebar = ()=>SidebarMenus.PluginsMenu(domain);
            var set = StorageService.Provider.GetPluginDefines();
            TheDomain = set.SingleOrDefault(s => s.Title == domain);
            TheAssembly = TheDomain?.GetJobSets().SingleOrDefault(s => s.ShortName == name);
        }

        protected override bool Content()
        {
            if (TheAssembly == null) return NoFound();
            return Nomal();
        }

        void WriteBar()
        {
            var (Name, Link) = Url.CreateRoute();
            var domainRoute = Url.CreateRoute(TheDomain);
            var bar = $@"<ol class='breadcrumb'>
                          <li><a href='{ Link }'>{ Name }</a></li>
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
            PageContent.WritePagerPanel(TheAssembly.ShortName, TheAssembly.GetJobs(), PageIndex, PageSize, d => Url.CreateRoute(TheDomain, TheAssembly, d));
            return true;
        }

      

    }

}

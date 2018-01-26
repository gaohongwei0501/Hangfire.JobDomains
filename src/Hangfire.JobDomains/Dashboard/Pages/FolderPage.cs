using Hangfire.Dashboard;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{

    internal class FolderPage : HtmlPage
    {

        public const string Title = "程序集详情";

        public PluginDefine ThePlugin { get; set; }

        public FolderPage(string name) 
        {
            FetchTitle = () => Title;
            FetchHeader = () => $"插件：{(ThePlugin == null ? name : ThePlugin.Title)}";

            Sidebar = ()=>SidebarMenus.PluginsMenu(name);
            var set = StorageService.Provider.GetPluginDefines();
            ThePlugin = set.SingleOrDefault(s => s.Title == name);
        }

        protected override bool Content()
        {
            if (ThePlugin == null) return NoFound();

            WriteBar();

            var servers = StorageService.Provider.GetServersByPlugin(ThePlugin.Title);
            var serverList = PageContent.Tag.ListLink(servers, Url.CreateServerRoute);
            var panel = PageContent.Tag.Panel("支持该服务的服务器", string.Empty, serverList);
            WriteLiteral(panel);

            var list = ThePlugin.GetJobSets();
            if (list.Count == 0) return None();
            PageContent.WritePagerPanel($@"任务程序集", list, PageIndex, PageSize, d => Url.CreateRoute(ThePlugin, d));
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
                          <li class='active'><a href='#'>{ Url.CreateRoute(ThePlugin).Name }</a></li>
                        </ol>";
            WriteLiteral(bar);
        }



    }
}

using Hangfire.Dashboard;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{
    internal class MainPage : HtmlPage
    {

        public const string Title = "任务包管理";

        public MainPage() 
        {
            FetchTitle = () => Title;
            FetchHeader = () => Title;
            Sidebar = SidebarMenus.DefaultMenu;
        }

        protected override bool Content()
        {
            var set = StorageService.Provider.GetPluginDefines();
            var list = set.OrderBy(s => s.Title);

            PageContent.WritePagerPanel("工作域任务包", list, PageIndex, PageSize, d => Url.CreateRoute(d));
            return true;
        }

    }

}

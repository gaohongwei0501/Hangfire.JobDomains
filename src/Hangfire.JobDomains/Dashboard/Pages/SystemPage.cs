using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{
    internal class SystemPage : HtmlPage
    {
        public const string Title = "参数设置";

        public SystemPage()
        {
            FetchTitle = () => Title;
            FetchHeader = () => Title;
            Sidebar = SidebarMenus.DefaultMenu;
        }

        protected override bool Content()
        {
            var content ="" ;
            var panel = PageContent.Tag.Panel("参数", string.Empty, content, string.Empty);
            WriteLiteral(panel);

            return true;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{
    internal class BatchSchedulePage : HtmlPage
    {
        public const string Title = "批量调度";

        public BatchSchedulePage()
        {
            FetchTitle = () => Title;
            FetchHeader = () => Title;
            Sidebar = SidebarMenus.DefaultMenu;
        }


        protected override bool Content()
        {
            return true;
        }
    }
}

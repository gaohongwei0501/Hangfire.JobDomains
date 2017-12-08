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
            var content = PageContent.Tag.CreateSysList() ;
            var panel = PageContent.Tag.Panel("参数", string.Empty, content, string.Empty);
            WriteLiteral(panel);

            var list = StorageService.Provider.GetJobCornSetting();
            var cronContent = PageContent.Tag.List(list, s => PageContent.Tag.ListItem($"{ s.Value }({ s.Key } min)", PageContent.Tag.CreateCronDeleteButtons(s.Key)));
            var cronPanel = PageContent.Tag.Panel("周期设置", string.Empty, cronContent, PageContent.Tag.CreateCronAddButtons());
            WriteLiteral(cronPanel);

            return true;
        }
    }
}

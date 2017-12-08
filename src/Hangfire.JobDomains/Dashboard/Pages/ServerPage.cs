using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{
    internal class ServerPage : HtmlPage
    {

        public const string Title = "任务服务器";

        public ServerDefine TheServer { get; private set; }

        public ServerPage(string name)
        {
            FetchTitle = () => Title;
            FetchHeader = () => TheServer == null ? name : TheServer.Name;

            Sidebar = () => SidebarMenus.ServersMenu(name);
            TheServer = StorageService.Provider.GetServer(name);
        }

        protected override bool Content()
        {
            var content = PageContent.Tag.CreateServerList(TheServer);
            var panel = PageContent.Tag.Panel("服务器参数", string.Empty, content, string.Empty);
            WriteLiteral(panel);

            var list = TheServer.Domains;
            var domainsContent = PageContent.Tag.ListLink(list, Url.CreateRoute);
            var domainsPanel = PageContent.Tag.Panel("支持插件", string.Empty, domainsContent);
            WriteLiteral(domainsPanel);

            return true;
        }
    }
}

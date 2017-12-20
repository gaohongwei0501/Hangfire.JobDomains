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

        public Lazy<ServerDefine> TheServer { get; private set; }

        public ServerPage(string name)
        {
            FetchTitle = () => Title;
            FetchHeader = () => $"服务器：{(TheServer.Value == null ? name : TheServer.Value.Name)}";

            Sidebar = () => SidebarMenus.ServersMenu(this, name);
            TheServer = new Lazy<ServerDefine>(() => StorageService.Provider.GetServer(this.Storage, name));
        }

        protected override bool Content()
        {
            var content = PageContent.Tag.CreateServerList(TheServer.Value);
            var panel = PageContent.Tag.Panel("服务器参数", string.Empty, content, string.Empty);
            WriteLiteral(panel);

            var queues = TheServer.Value.Queues;
            var queuesContent = PageContent.Tag.ListLink(queues, Url.CreateQueueRoute);
            var queuesPanel = PageContent.Tag.Panel("所属队列", string.Empty, queuesContent);
            WriteLiteral(queuesPanel);

            var domains = TheServer.Value.Domains;
            var domainsContent = PageContent.Tag.ListLink(domains, Url.CreateRoute);
            var domainsPanel = PageContent.Tag.Panel("支持插件", string.Empty, domainsContent);
            WriteLiteral(domainsPanel);

            return true;
        }
    }
}

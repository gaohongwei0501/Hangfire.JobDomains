using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
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
            var server = TheServer.Value;
            var desc = PageContent.Tag.CreateDescription(server.Description);
            WriteLiteral(desc);

            var content = PageContent.Tag.CreateServerList(server);
            var panel = PageContent.Tag.Panel("服务器参数", string.Empty, content, string.Empty, customAttr: $" data-url='{Url.To(UrlHelperExtension.ServerCommandRoute)}' data-server='{server.Name}' data-path='{server.PlugPath}' ");
            WriteLiteral(panel);

            var queues = server.Queues;
            var queuesContent = PageContent.Tag.ListLink(queues, Url.CreateQueueRoute);
            var queuesPanel = PageContent.Tag.Panel("所属队列", string.Empty, queuesContent);
            WriteLiteral(queuesPanel);

            var domains = server.Plugins;
            var domainsContent = PageContent.Tag.ListLink(domains, Url.CreateRoute);
            var pluginsPanel = PageContent.Tag.Panel("支持插件", string.Empty, domainsContent);
            WriteLiteral(pluginsPanel);

            WriteLiteral(PageContent.Tag.CreateCommandComfirmBox());
            PageContent.WriteScriptFile(Url.To("/jsex/domainScript"));
            PageContent.WriteScript(@"serverScriptStart();");
            return true;
        }
    }
}

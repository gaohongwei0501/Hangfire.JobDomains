using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{


    internal class QueuePage : HtmlPage
    {

        public const string Title = "队列详情";

        public Lazy<QueueDefine> TheQueue { get; private set; }

        public QueuePage(string name)
        {
            FetchTitle = () => Title;
            FetchHeader = () => $"队列：{(TheQueue.Value == null ? name : TheQueue.Value.Name)}";
            Sidebar = () => SidebarMenus.QueuesMenu(this, name);
            TheQueue = new Lazy<QueueDefine>(() => StorageService.Provider.GetQueue(this.Storage, name));
        }

        protected override bool Content()
        {
            var list = StorageService.Provider.GetServersByQueue(this.Storage, TheQueue.Value.Name);
            var domainsContent = PageContent.Tag.ListLink(list, Url.CreateServerRoute);
            var domainsPanel = PageContent.Tag.Panel("队列服务器", string.Empty, domainsContent);
            WriteLiteral(domainsPanel);

            return true;
        }
    }

}

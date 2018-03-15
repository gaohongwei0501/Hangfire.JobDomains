using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{
    internal class QueueListPage : HtmlPage
    {
        public const string Title = "插件队列";

        public QueueListPage()
        {
            FetchTitle = () => Title;
            FetchHeader = () => Title;
            Sidebar = SidebarMenus.DefaultMenu;
        }

        protected override bool Content()
        {
            var list = StorageService.Provider.GetQueues(this.Storage);
            var content = PageContent.Tag.ListLink(list, Url.CreateQueueRoute);
            var panel = PageContent.Tag.Panel("队列列表", string.Empty, content);
            WriteLiteral(panel);
            return true;
        }
    }
}

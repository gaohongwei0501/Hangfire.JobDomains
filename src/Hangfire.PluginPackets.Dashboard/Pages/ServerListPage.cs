using Hangfire.PluginPackets.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{
    internal class ServerListPage : HtmlPage
    {
        public const string Title = "任务包服务器";

        public ServerListPage()
        {
            FetchTitle = () => Title;
            FetchHeader = () => Title;
            Sidebar = SidebarMenus.DefaultMenu;
        }

        protected override bool Content()
        {
            var list = StorageService.Provider.GetServers(this.Storage);
            var content = PageContent.Tag.ListLink(list, Url.CreateServerRoute);
            var panel = PageContent.Tag.Panel("服务器列表", string.Empty, content);
            WriteLiteral(panel);
            return true;
        }

    
    }
}

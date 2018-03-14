using Hangfire.PluginPackets.Dynamic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{
    internal class ClientPage : HtmlPage
    {
        public const string Title = "UI服务器";

        public ClientPage()
        {
            FetchTitle = () => Title;
            FetchHeader = () => $"当前UI服务器";
            Sidebar = () => SidebarMenus.DefaultMenu();
        }

        protected override bool Content()
        {

            var item = PageContent.Tag.ListItem($" 动态程序集位置：{TypeFactory.DynamicPath}", $"<span class='js-client-dynamic-create cmd-link' data-cmd='{ ClientPageCommand.Refresh }' >重新生成</span>");
            var content = PageContent.Tag.List(item);

            var panel = PageContent.Tag.Panel("客户端设置", string.Empty, content, string.Empty, customAttr: $" data-url='{Url.To(UrlHelperExtension.ClientCommandRoute)}'  ");
            WriteLiteral(panel);


            WriteLiteral(PageContent.Tag.CreateCommandComfirmBox());
            PageContent.WriteScriptFile(Url.To("/jsex/domainScript"));
            PageContent.WriteScript(@"clientScriptStart();");
            return true;
        }
    }
}

using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{
  

    internal class QueuePage : HtmlPage
    {

        public const string Title = "任务服务器";

        public QueueDefine TheQueue { get; private set; }

        public QueuePage(string name)
        {
            FetchTitle = () => Title;
            FetchHeader = () => $"队列：{(TheQueue == null ? name : TheQueue.Name)}";

            Sidebar = () => SidebarMenus.QueuesMenu(name);
            TheQueue = StorageService.Provider.GetQueue(name);
        }

        protected override bool Content()
        {
            var list = TheQueue.Servers;
            var domainsContent = PageContent.Tag.ListLink(list, Url.CreateServerRoute);
            var domainsPanel = PageContent.Tag.Panel("队列服务器", string.Empty, domainsContent);
            WriteLiteral(domainsPanel);

            return true;
        }
    }

}

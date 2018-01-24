using Hangfire.Dashboard;
using Hangfire.JobDomains.Dashboard.Pages;
using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard
{
    internal static class UrlHelperExtension
    {

        public static string baseRoute = "packets";

        static string EscapeRoute(this string input)
        {
            return input.Replace(".", "~");
        }

        static string EscapeNomal(this string input)
        {
            return input.Replace("~", ".");
        }

        static string EscapeNomal(this Group input)
        {
            return input.Value.Replace("~", ".");
        }

        #region MainPage

        public static string MainPageRoute => $"/{baseRoute}";

        /// <summary>
        /// MainPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url)
        {
            var name = MainPage.Title;
            var link = MainPageRoute;
            return (name, Url.To(link));
        }


        #endregion

        #region SystemPage

        public static string SystemPageRoute => $"/{baseRoute}/set";

        /// <summary>
        /// MainPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateSystemRoute(this UrlHelper Url)
        {
            var name = SystemPage.Title; 
            var link = SystemPageRoute;
            return (name, Url.To(link));
        }

        #endregion

        #region BatchSchedulePage

        public static string BatchSchedulePageRoute = $"/{baseRoute}/batch";

        /// <summary>
        /// BatchSchedulePage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateBatchRoute(this UrlHelper Url)
        {
            var name = BatchSchedulePage.Title;
            var link = BatchSchedulePageRoute;
            return (name, Url.To(link));
        }

        #endregion

        #region ServerListPage

        public static string ServerListPageRoute => $"/{baseRoute}/servers";

        /// <summary>
        /// ServerListPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateServerListRoute(this UrlHelper Url)
        {
            var name = ServerListPage.Title;
            var link = ServerListPageRoute;
            return (name, Url.To(link));
        }

        #endregion

        #region ServerPage

        public static string ServerPageRoute = $"/{baseRoute}/server-(?<name>.+)";

        /// <summary>
        /// ServerPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateServerRoute(this UrlHelper Url, ServerDefine server)
        {
            var name = server.Name;
            var link = ServerPageRoute.Replace("(?<name>.+)", name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static (string Name, string Link) CreateServerRoute(this UrlHelper Url, string serverName)
        {
            var name = serverName;
            var link = ServerPageRoute.Replace("(?<name>.+)", serverName).EscapeRoute();
            return (name, Url.To(link));
        }

        public static ServerPage CreateServerPage(Match match)
        {
            var name = match.Groups["name"];
            return new ServerPage(name.EscapeNomal());
        }

        #endregion

        #region QueueListPage

        public static string QueueListPageRoute = $"/{baseRoute}/queues";

        /// <summary>
        /// QueueListPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateQueueListRoute(this UrlHelper Url)
        {
            var name = QueueListPage.Title;
            var link = QueueListPageRoute;
            return (name, Url.To(link));
        }

        #endregion

        #region QueuePage

        public static string QueuePageRoute = $"/{baseRoute}/queue-(?<name>.+)";

        /// <summary>
        /// QueuePage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateQueueRoute(this UrlHelper Url, QueueDefine queue)
        {
            var name = queue.Name;
            var link = QueuePageRoute.Replace("(?<name>.+)", name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static (string Name, string Link) CreateQueueRoute(this UrlHelper Url, string queueName)
        {
            var name = queueName;
            var link = QueuePageRoute.Replace("(?<name>.+)", queueName).EscapeRoute();
            return (name, Url.To(link));
        }

        public static QueuePage CreateQueuePage(Match match)
        {
            var name = match.Groups["name"];
            return new QueuePage(name.EscapeNomal());
        }

        #endregion

        #region FolderPage

        public static string FolderPageRoute = $"/{baseRoute}/folder-(?<name>.+)";

        /// <summary>
        /// FolderPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url , PluginDefine plugin)
        {
            var name = plugin.Title;
            var link = FolderPageRoute.Replace("(?<name>.+)", plugin.PathName).EscapeRoute();
            return (name, Url.To(link));
        }

        public static FolderPage CreateFolderPage(Match match)
        {
            var name = match.Groups["name"];
            return new FolderPage(name.EscapeNomal());
        }

        #endregion

        #region AssemblyPage

        public static string AssemblyPageRoute = $"/{baseRoute}/assembly-(?<name>.+)-folder-(?<plugin>.+)";

        /// <summary>
        /// AssemblyPage  CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url, PluginDefine plugin, AssemblyDefine assembly)
        {
            var name = assembly.Title;
            var link = AssemblyPageRoute.Replace("(?<plugin>.+)", plugin.PathName).Replace("(?<name>.+)", assembly.ShortName).EscapeRoute();
            return (name, Url.To(link));
        }

        public static AssemblyPage CreateAssemblyPage(Match match)
        {
            var plugin = match.Groups["plugin"];
            var name = match.Groups["name"];
            return new AssemblyPage(plugin.EscapeNomal(), name.EscapeNomal());
        }

        #endregion

        #region JobPage

        public static string JobPageRoute = $"/{baseRoute}/job-(?<name>.+)-assembly-(?<assembly>.+)-folder-(?<plugin>.+)";

        /// <summary>
        /// JobPage  CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url, PluginDefine plugin, AssemblyDefine assembly, JobDefine job)
        {
            var name = job.Title;
            var link = JobPageRoute.Replace("(?<plugin>.+)", plugin.PathName).Replace("(?<assembly>.+)", assembly.ShortName).Replace("(?<name>.+)", job.Name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static JobPage CreateJobPage(Match match)
        {
            var plugin = match.Groups["plugin"];
            var assembly = match.Groups["assembly"];
            var name = match.Groups["name"];
            return new JobPage(plugin.EscapeNomal(), assembly.EscapeNomal(), name.EscapeNomal());
        }

        #endregion

        #region CommandPage

        public static string PluginCommandRoute = $"/{baseRoute}/command-plugin";

        public static string JobCommandRoute = $"/{baseRoute}/command-job";

        public static string ServerCommandRoute = $"/{baseRoute}/command-server";

        #endregion

    }
}

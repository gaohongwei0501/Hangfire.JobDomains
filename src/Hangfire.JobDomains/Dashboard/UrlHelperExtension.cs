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

        public const string MainPageRoute = "/domains";

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

        public const string SystemPageRoute = "/domains/set";

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

        public const string BatchSchedulePageRoute = "/domains/batch";

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

        #region DomainPage

        public const string DomainPageRoute = "/domains/domain-(?<name>.+)";

        /// <summary>
        /// DomainPage CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url , DomainDefine domain)
        {
            var name = domain.Name;
            var link = DomainPageRoute.Replace("(?<name>.+)", name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static DomainPage CreateDomainPage(Match match)
        {
            var name = match.Groups["name"];
            return new DomainPage(name.EscapeNomal());
        }

        #endregion

        #region AssemblyPage

        public const string AssemblyPageRoute = "/domains/assembly-(?<name>.+)-domain-(?<domain>.+)";

        /// <summary>
        /// AssemblyPage  CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url, DomainDefine domain, AssemblyDefine assembly)
        {
            var domainName = domain.Name;
            var name = assembly.Title;
            var link = AssemblyPageRoute.Replace("(?<domain>.+)", domainName).Replace("(?<name>.+)", name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static AssemblyPage CreateAssemblyPage(Match match)
        {
            var domain = match.Groups["domain"];
            var name = match.Groups["name"];
            return new AssemblyPage(domain.EscapeNomal(), name.EscapeNomal());
        }

        #endregion

        #region JobPage

        public const string JobPageRoute = "/domains/job-(?<name>.+)-assembly-(?<assembly>.+)-domain-(?<domain>.+)";


        /// <summary>
        /// JobPage  CreateRoute
        /// </summary>
        public static (string Name, string Link) CreateRoute(this UrlHelper Url, DomainDefine domain, AssemblyDefine assembly, JobDefine job)
        {
            var name = job.Title;
            var link = JobPageRoute.Replace("(?<domain>.+)", domain.Name).Replace("(?<assembly>.+)", assembly.ShortName).Replace("(?<name>.+)", job.Name).EscapeRoute();
            return (name, Url.To(link));
        }

        public static JobPage CreateJobPage(Match match)
        {
            var domain = match.Groups["domain"];
            var assembly = match.Groups["assembly"];
            var name = match.Groups["name"];
            return new JobPage(domain.EscapeNomal(), assembly.EscapeNomal(), name.EscapeNomal());
        }

        #endregion

        #region DomainCommandPage

     //   public const string DomainCommandRoute = "/domains/command-domain-(?<cmd>.+)-domain-(?<name>.+)";
        public const string DomainCommandRoute = "/domains/command-domain";

        #endregion

        #region JobCommandPage

        // public const string JobCommandPageRoute = "/domains/command-job-(?<cmd>.+)-job-(?<name>.+)-assembly-(?<assembly>.+)-domain-(?<domain>.+)";
        public const string JobCommandRoute = "/domains/command-job";
        
        #endregion

    }
}

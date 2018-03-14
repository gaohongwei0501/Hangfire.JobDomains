using Hangfire.Common;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using Hangfire.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dashboard.Pages
{

    internal class JobPage : HtmlPage
    {

        public const string Title = "任务详情";

        public PluginDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public JobDefine TheJob { get; set; }

        public JobPage(string domain, string assembly, string name)
        {
            FetchTitle = () => "任务详情";
            FetchHeader = () =>$"任务：{(TheJob == null ? name : TheJob.Title)}";
            Sidebar = () => SidebarMenus.JobsMenu(domain, assembly, name);

            var set = StorageService.Provider.GetPluginDefines();
            TheDomain = set.SingleOrDefault(s => s.Title == domain);
            TheAssembly = TheDomain?.GetJobSets().SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly?.GetJobs().SingleOrDefault(s => s.Name == name);
        }

        protected override bool Content()
        {
            if (TheJob == null) return NoFound();
            return Nomal();
        }

        void WriteBar()
        {
            var (Name, Link) = Url.CreateRoute();
            var domainRoute = Url.CreateRoute(TheDomain);
            var assemblyRoute = Url.CreateRoute(TheDomain, TheAssembly);

            var bar = $@"<ol class='breadcrumb'>
                          <li><a href='{ Link }'>{ Name }</a></li>
                          <li><a href='{ domainRoute.Link }'>{ domainRoute.Name }</a></li>
                          <li><a href='{ assemblyRoute.Link }'>{ assemblyRoute.Name }</a></li>
                          <li class='active'><a href='#'>{ Url.CreateRoute(TheDomain, TheAssembly, TheJob).Name }</a></li>
                        </ol>";
            WriteLiteral(bar);
        }

        bool NoFound()
        {
            WriteLiteral($@"<div class=""alert alert-danger"" role=""alert"">预制任务未被发现</div>");
            return false;
        }

        bool Nomal()
        {
            WriteBar();

            //using (var connection = Storage.GetConnection())
            //{
            //    var storageConnection = connection as JobStorageConnection;
            //    if (storageConnection != null)
            //    {
            //        var recurringJobs = storageConnection.GetRecurringJobs(1,100);
            //    }
            //    else
            //    {
            //        var recurringJobs = connection.GetRecurringJobs();
            //    }

            //    var hash = connection.GetAllEntriesFromHash($"recurring-job:{"测试任务___1111"}");
            //    var invocationData = JobHelper.FromJson<Hangfire.PluginPackets._Helper.InvocationData>(hash["Job"]);

            //  //  var invocationData = new InvocationData("", "", "", "");
            //    var Job = invocationData.Deserialize();

            //}


            //var historyContent = PageContent.Tag.CreateJobHistoryList(TheDomain, TheAssembly, TheJob);
            //var historyPanel = PageContent.Tag.Panel("任务调取历史", string.Empty, historyContent, string.Empty);
            //WriteLiteral(historyPanel);

            var structures = TheJob.GetConstructors();

            var customAttr=$@" data-domain=""{ TheDomain.Title }""  data-assembly=""{ TheAssembly.ShortName }"" data-job=""{ TheJob.Name }""  data-url=""{Url.To(UrlHelperExtension.JobCommandRoute)}"" ";
            var queues = StorageService.Provider.GetQueuesByPlugin(this.Storage, TheDomain.PathName);

            foreach (var structure in structures)
            {
                var id = Guid.NewGuid().ToString();
                StringBuilder title = new StringBuilder();
                StringBuilder inputs = new StringBuilder();
                foreach (var parameterInfo in structure.Paramers)
                {
                    title.Append($"{ parameterInfo.Type } { parameterInfo.Name },");
                    var parameterId = $"{ id }_{ parameterInfo.Name }";
                    var name = $"{ parameterInfo.Name }({ parameterInfo.Type })";
                    var dataTag = $@" data-name=""{ parameterInfo.Name }""  data-type=""{ parameterInfo.Type }""";

                    if (parameterInfo.Type == typeof(string).Name)
                    {
                        inputs.Append(PageContent.Tag.InputTextbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else if (parameterInfo.Type == typeof(int).Name)
                    {
                        inputs.Append(PageContent.Tag.InputNumberbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else if (parameterInfo.Type == typeof(DateTime).Name)
                    {
                        inputs.Append(PageContent.Tag.InputDatebox(parameterId, name, parameterInfo.Name,"" ,dataTag));
                    }
                    else if (parameterInfo.Type == typeof(bool).Name)
                    {
                        inputs.Append("<br/>" + PageContent.Tag.InputCheckbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else
                    {
                        continue;
                    }
                }

                var heading = $"{TheJob.Name}({title.ToString().TrimEnd(',')})";

                var queue = PageContent.Tag.CreateJobQueues(queues);
                var test = PageContent.Tag.CreateJobTestButton();
                var schedule = PageContent.Tag.CreateJobScheduleButton( );
                var period = PageContent.Tag.CreateJobPeriodButton( TheJob.Title);

                var panel = PageContent.Tag.Panel(heading, "", inputs.ToString(), new List<string> { queue, test, schedule, period } , "js-domain-job", customAttr);
                WriteLiteral(panel);
            }

            WriteLiteral(PageContent.Tag.CreateCommandComfirmBox());
            PageContent.WriteScriptFile(Url.To("/jsex/domainScript"));
            PageContent.WriteScript(@"jobScriptStart();");
            return true;
        }

    }

}

using Hangfire.JobDomains.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Pages
{

    internal class JobPage : HtmlPage
    {

        public const string Title = "任务详情";

        public DomainDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public JobDefine TheJob { get; set; }

        public JobPage(string domain, string assembly, string name)
        {
            FetchTitle = () => "任务详情";
            FetchHeader = () => TheJob == null ? name : TheJob.Name;
            Sidebar = () => SidebarMenus.JobsMenu(domain, assembly, name);

            var set = JobDomainManager.GetDomainDefines();
            TheDomain = set.SingleOrDefault(s => s.Name == domain);
            TheAssembly = TheDomain == null ? null : TheDomain.JobSets.SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly == null ? null : TheAssembly.Jobs.SingleOrDefault(s => s.Name == name);
        }

        protected override bool Content()
        {
            if (TheJob == null) return NoFound();
            return Nomal();
        }

        void WriteBar()
        {
            var mainRoute = Url.CreateRoute();
            var domainRoute = Url.CreateRoute(TheDomain);
            var assemblyRoute = Url.CreateRoute(TheDomain, TheAssembly);

            var bar = $@"<ol class='breadcrumb'>
                          <li><a href='{ mainRoute.Link }'>{ mainRoute.Name }</a></li>
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

            var structures = TheJob.Type.GetConstructors();

            var customAttr=$@" data-domain=""{ TheDomain.Name }""  data-assembly=""{ TheAssembly.ShortName }"" data-job=""{ TheJob.Name }""  data-url=""{Url.To(UrlHelperExtension.JobCommandRoute)}"" ";

            foreach (var structure in structures)
            {
                var id = Guid.NewGuid().ToString();
                StringBuilder title = new StringBuilder();
                StringBuilder inputs = new StringBuilder();
                foreach (var parameterInfo in structure.GetParameters())
                {
                    title.Append($"{parameterInfo.ParameterType.Name} {parameterInfo.Name},");
                    var parameterId = $"{id}_{parameterInfo.Name}";
                    var name = $"{parameterInfo.Name}({ parameterInfo.ParameterType.Name })";
                    var dataTag = $@" data-name=""{parameterInfo.Name}""  data-type=""{parameterInfo.ParameterType.Name}""";

                    if (parameterInfo.ParameterType == typeof(string))
                    {
                        inputs.Append(PageContent.Tag.InputTextbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else if (parameterInfo.ParameterType == typeof(int))
                    {
                        inputs.Append(PageContent.Tag.InputNumberbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else if (parameterInfo.ParameterType == typeof(DateTime))
                    {
                        inputs.Append(PageContent.Tag.InputDatebox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else if (parameterInfo.ParameterType == typeof(bool))
                    {
                        inputs.Append("<br/>" + PageContent.Tag.InputCheckbox(parameterId, name, parameterInfo.Name, dataTag));
                    }
                    else
                    {
                        continue;
                    }
                }

                var heading = $"{TheJob.Name}({title.ToString().TrimEnd(',')})";
                var cmdButtons = PageContent.Tag.CreateJobScheduleButtons(id);
                var panel = PageContent.Tag.Panel(heading, "", inputs.ToString(), cmdButtons, "js-domain-job", customAttr);
                WriteLiteral(panel);
            }

            WriteLiteral(PageContent.Tag.CreateCommandComfirmBox());
            PageContent.WriteScriptFile(Url.To("/jsex/domainJob"));
            PageContent.WriteScript(@"jobDoStart();");
            return true;
        }

    }

}

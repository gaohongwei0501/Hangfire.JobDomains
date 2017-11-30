using Hangfire.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.PageResult
{

    internal class CommandResult: ContentResult
    {

        public Func<(string Name, string Link)> CreateSuccessRoute { get; set; } = () => ("","#");

        public Func<(string Name, string Link)> CreateExceptionRoute { get;set; } = () => ("", "#");

        public Func<bool> InvokeCommand { get; set; } = () => false;

        public CommandResult(HtmlPage page) :base(page)
        {
            base.Content = TryContent;
        }

        public void TryContent()
        {
            try
            {
                var result = ExecuteCommand();
                var routeItem = CreateSuccessRoute();
                if (result) Page.HtmlWrite($@"<div class=""alert alert-success"" role=""alert"">任务域指令执行成功！<a href=""{ routeItem.Link }"" class=""alert-link"">返回</a></div>");
                // WriteScript($@"alert(""{ result}"")");
            }
            catch (Exception ex)
            {
                Page.HtmlWrite($@"<div class=""alert alert-danger"" role=""alert""> CommandResult 渲染遇见异常：{ ex.Message }.</div>");
            }
        }

        bool ExecuteCommand()
        {
            try
            {
                return InvokeCommand();
            }
            catch (Exception ex)
            {
                var routeItem = CreateExceptionRoute();
                Page.HtmlWrite($@"<div class=""alert alert-danger"" role=""alert"">任务域指令遇见异常：{ ex.Message }.<a href=""{ routeItem.Link }"" class=""alert-link"">返回</a></div>");
            }
            return false;
        }

        
    }
}

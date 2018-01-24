using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Server;
using Hangfire.JobDomains.Storage;
using Hangfire.States;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using  CronExpressionDescriptor;

namespace Hangfire.JobDomains.Dashboard.Dispatchers
{



    internal class JobCommandDispatcher : CommandDispatcher<JsonData>
    {

        public override Task<JsonData> Exception(Exception ex)
        {
            return Task.FromResult(new JsonData(ex, null));
        }

        public PluginDefine TheDomain { get; set; }

        public AssemblyDefine TheAssembly { get; set; }

        public JobDefine TheJob { get; set; }

        public Dictionary<string, object> JobData { get; set; }

        public override async Task<JsonData> Invoke()
        {
            var cmd = await GetFromValue("cmd");
            var jobCmd = JobPageCommand.None;
            Enum.TryParse<JobPageCommand>(cmd, out jobCmd);

            var domain = await GetFromValue("domain");
            var assembly = await GetFromValue("assembly");
            var job = await GetFromValue("job");

            var start = await GetFromValue<DateTime>("start", DateTime.MinValue);
            var period = await GetFromValue("period");
            var queue = (await GetFromValue("queue")).ToLower();
            var jobSign = await GetFromValue("sign");

            JobData = await GetDictionaryValue("data");
            var paramers = JobData?.Select(s => s.Value).ToArray();

            var set = StorageService.Provider.GetPluginDefines();
            TheDomain = set.SingleOrDefault(s => s.Title == domain);
            TheAssembly = TheDomain?.GetJobSets().SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly?.GetJobs().SingleOrDefault(s => s.Name == job);

            if (TheJob == null) throw (new Exception("未正确定位到工作任务."));
            if (jobCmd == JobPageCommand.None) throw (new Exception("未正确定位到任务指令."));

            switch (jobCmd)
            {
                case JobPageCommand.Schedule: Schedule(queue, start, period, jobSign, paramers); break;
                case JobPageCommand.Delay: Delay(queue, start, paramers); break;
                case JobPageCommand.Immediately: JobTest(queue, paramers); break;
                case JobPageCommand.Test: JobTest(queue, paramers); break;
            }

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }

        void JobTest(string queue, object[] paramers)
        {
            JobInvoke.Test(queue, TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

        bool IsPeriod(string period) {
            try
            {
                ExpressionDescriptor.GetDescription(period);
                return true;
            }
            catch {
                return false;
            }
        }


        void Schedule(string queue, DateTime start, string period, string jobSign, object[] paramers)
        {
            if(IsPeriod(period)==false) throw (new Exception("任务周期不能被识别"));
            if (start < DateTime.Now) throw (new Exception("任务启动时间设置失败"));
            var delay = start - DateTime.Now;
            JobInvoke.ScheduleEnqueued(delay, period, queue, jobSign, TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

        void Delay(string queue, DateTime start, object[] paramers)
        {
            if (start < DateTime.Now) throw (new Exception("任务启动时间设置失败"));
            var delay = start - DateTime.Now;
            //  BackgroundJob.Schedule(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), delay);
            JobInvoke.DelayEnqueued(delay, queue, TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

   
    }
}

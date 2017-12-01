using Hangfire.JobDomains.AppSetting;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Server;
using Hangfire.JobDomains.Storage;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Dashboard.Dispatchers
{
    internal class JobCommandDispatcher : CommandDispatcher<JsonData>
    {

        public override Task<JsonData> Exception(Exception ex)
        {
            return Task.FromResult(new JsonData(ex, null));
        }

        public DomainDefine TheDomain { get; set; }

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

            var start = await GetFromValue<DateTime>("start",DateTime.MinValue);
            var period = await GetFromValue<int>("period", 0);

            JobData = await GetDictionaryValue("data");
            if (JobData.Count == 0) throw (new Exception("提交任务操作参数不齐全."));
            var paramers = JobData == null ? null : JobData.Select(s => s.Value).ToArray();

            var set = StorageService.Provider.GetDomainDefines();
            TheDomain = set.SingleOrDefault(s => s.Name == domain);
            TheAssembly = TheDomain == null ? null : TheDomain.JobSets.SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly == null ? null : TheAssembly.Jobs.SingleOrDefault(s => s.Name == job);

            if (TheJob == null) throw (new Exception("未正确定位到工作任务."));
            if (jobCmd == JobPageCommand.None) throw (new Exception("未正确定位到任务指令."));

            switch (jobCmd)
            {
                case JobPageCommand.Schedule: Schedule(start, period, paramers); break;
                case JobPageCommand.Delay: Delay(start, paramers); break;
                case JobPageCommand.Immediately: Immediately(paramers); break;
            }

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }

        void Schedule(DateTime start, int period, object[] paramers)
        {
            if (start < DateTime.Now) throw (new Exception("任务启动时间设置失败"));
            if (JobCornSetting.Dictionary.Contains(period) == false) throw (new Exception("任务周期设置设置失败"));
            RecurringJob.AddOrUpdate(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), Cron.MinuteInterval(period));
        }

        void Delay(DateTime start, object[] paramers)
        {
            if (start < DateTime.Now) throw (new  Exception("任务启动时间设置失败"));
            var delay = start - DateTime.Now;
            BackgroundJob.Schedule(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), delay);
        }

        void Immediately(object[] paramers)
        {
            BackgroundJob.Enqueue(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers));
        }

    }
}

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
            var paramers = JobData == null ? null : JobData.Select(s => s.Value).ToArray();

            var set = StorageService.Provider.GetDomainDefines();
            TheDomain = set.SingleOrDefault(s => s.Title == domain);
            TheAssembly = TheDomain == null ? null : TheDomain.GetJobSets().SingleOrDefault(s => s.ShortName == assembly);
            TheJob = TheAssembly == null ? null : TheAssembly.GetJobs().SingleOrDefault(s => s.Name == job);

            if (TheJob == null) throw (new Exception("未正确定位到工作任务."));
            if (jobCmd == JobPageCommand.None) throw (new Exception("未正确定位到任务指令."));

            switch (jobCmd)
            {
                case JobPageCommand.Schedule: Schedule(start, period, paramers); break;
                case JobPageCommand.Delay: Delay(start, paramers); break;
                case JobPageCommand.Immediately: Immediately(paramers); break;
                case JobPageCommand.Test: JobTest(paramers); break;
            }

            return new JsonData
            {
                IsSuccess = true,
                Message = "提交成功.",
                Url = "",
            };
        }

        void JobTest(object[] paramers)
        {
            JobInvoke.Test(TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

        void Schedule(DateTime start, int period, object[] paramers)
        {
            if (start < DateTime.Now) throw (new Exception("任务启动时间设置失败"));
            var set = StorageService.Provider.GetJobCornSetting();
            if (set.ContainsKey(period) == false) throw (new Exception("任务周期设置设置失败"));
            // RecurringJob.AddOrUpdate(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), Cron.MinuteInterval(period), queue: TheDomain.Name.ToLower());
            var delay = start - DateTime.Now;
            JobInvoke.ScheduleEnqueued(delay, period, TheDomain.Title.ToLower(), TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

        void Delay(DateTime start, object[] paramers)
        {
            if (start < DateTime.Now) throw (new Exception("任务启动时间设置失败"));
            var delay = start - DateTime.Now;
            //  BackgroundJob.Schedule(() => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), delay);
            JobInvoke.DelayEnqueued(delay, TheDomain.Title.ToLower(), TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

        void Immediately(object[] paramers)
        {
            //IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            //EnqueuedState state = new Hangfire.States.EnqueuedState(TheDomain.Name.ToLower());
            //hangFireClient.Create<JobInvoke>(c => JobInvoke.Invoke(TheDomain.BasePath, TheAssembly.FullName, TheJob.FullName, paramers), state);
            JobInvoke.ImmediatelyEnqueued(TheDomain.Title.ToLower(), TheDomain.PathName, TheAssembly.FullName, TheJob.FullName, paramers);
        }

    }
}

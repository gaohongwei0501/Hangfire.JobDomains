using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{

    public class 添加Web测试任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<WebJobExecute>(paramer, "GetTestService");
    }

    public class 添加Web排期任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<WebJobExecute>(paramer, "GetScheduleService");
    }

    public class 添加Web即时任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<WebJobExecute>(paramer, "GetEnqueuedService");
    }

    public class 添加Web计划任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<WebJobExecute>(paramer, "GetPeriodService");
    }
}

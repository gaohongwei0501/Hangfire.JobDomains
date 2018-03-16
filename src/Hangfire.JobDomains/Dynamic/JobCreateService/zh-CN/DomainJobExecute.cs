using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    public class 添加插件测试任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<DomainJobExecute>(paramer, "GetTestService");
    }

    public class 添加插件排期任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<DomainJobExecute>(paramer, "GetScheduleService");
    }

    public class 添加插件即时任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<DomainJobExecute>(paramer, "GetEnqueuedService");
    }

    public class 添加插件计划任务 : JobCreateService
    {
        public override void Create(PluginParamer paramer) => CreateJob<DomainJobExecute>(paramer, "GetPeriodService");
    }

}

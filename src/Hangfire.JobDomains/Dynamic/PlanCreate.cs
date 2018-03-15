using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.PluginPackets.Interface;
using Hangfire.States;

namespace Hangfire.PluginPackets.Dynamic
{
    public class PlanCreate
    {

        public static void AddNewPlan(PluginParamer paramer)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
            hangFireClient.Create<AddNewPlanJob>(service => service.CreatePlanType(paramer), state);
        }

    }

    public class AddNewPlanJob
    {
        public void CreatePlanType(PluginParamer paramer)
        {
            TypeFactory.CreateType<JobExecute>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
        }
    }

    public class 添加计划任务
    {
        public void CreatePlanType(PluginParamer paramer)
        {
            TypeFactory.CreateType<JobExecute>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
        }
    }
}

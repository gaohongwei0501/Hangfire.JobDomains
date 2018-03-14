using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Models
{
    public class QueuePlanDefine
    {

        public QueuePlanDefine() { }
        public QueuePlanDefine(string plug, string assembly, string job, string queue, string plan, string cron, object[] args)
        {
            PlugName = plug;
            AssemblyName = assembly;
            TypeName = job;
            QueueName = queue;
            PlanName = plan;
            Period = cron;
            Args = args;
        }

        public string PlanName { get; set; }

        public string QueueName { get; set; }

        public string Period { get; set; }

        public string PlugName { get; set; }

        public string AssemblyName { get; set; }

        public string TypeName { get; set; }

        public object[] Args { get; set; }

    }
}

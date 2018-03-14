using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.Entities
{
    public class QueuePlan 
    {
        public QueuePlan() { }

        [Key]
        public string PlanName { get; set; }

        public string QueueName { get; set; }

        public DateTime CreatedAt { get; set; }

        public string Period { get; set; }

        public string PlugName { get; set; }

        public string AssemblyName { get; set; }

        public string TypeName { get; set; }

        public string Args { get; set; }


    }
}

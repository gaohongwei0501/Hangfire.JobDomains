using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{
    

    /// <summary>
    /// 周期任务批次
    /// </summary>
    /// <typeparam name="T">任务类型</typeparam>
    public class PeriodBatch<T> : IPeriodBatch where T : IPrefabrication
    {
        public PeriodBatch(string title, string cron, string queue = "",params object[] args)
        {
            BatchTitle = title;
            BatchCron = cron;
            BatchQueue = queue;
            JobParamers = args;
        }

        /// <summary>
        /// 名称
        /// </summary>
        public string BatchTitle { get; }

        /// <summary>
        /// 队列
        /// </summary>
        public string BatchQueue { get; }

        /// <summary>
        /// 周期
        /// </summary>
        public string BatchCron { get; }

        /// <summary>
        /// 任务参数
        /// </summary>
        public object[] JobParamers { get; set; }
    }



}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{
    /// <summary>
    /// 任务批次
    /// </summary>
    public interface IJobBatch
    {

        /// <summary>
        /// 名称
        /// </summary>
        string BatchTitle { get; }

        /// <summary>
        /// 队列
        /// </summary>
        string BatchQueue { get; }

        /// <summary>
        /// 任务参数
        /// </summary>
        object[] JobParamers { get;  }

    }

    public interface IPeriodBatch : IJobBatch
    {
        /// <summary>
        /// 周期
        /// </summary>
        string BatchCron { get; }
    }
}

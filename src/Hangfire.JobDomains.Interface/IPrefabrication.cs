using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Interface
{

    /// <summary>
    /// 预制任务
    /// </summary>
    public interface IPrefabrication
    {

        /// <summary>
        /// 任务测试
        /// </summary>
        /// <returns></returns>
        bool Test();

        /// <summary>
        /// 任务执行
        /// </summary>
        /// <returns></returns>
        void Dispatch();

    }
}

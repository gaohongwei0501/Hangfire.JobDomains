using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    /// <summary>
    /// 网络任务执行
    /// </summary>
    public class WebJobExecute : JobExecute
    {
        protected override T Execute<T>(PluginParamer paramer, Action act, Func<AppDomain, T> GetResult)
        {
            return default(T);
        }
    }
}

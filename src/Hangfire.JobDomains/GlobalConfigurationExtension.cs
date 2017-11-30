using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Hangfire.Dashboard;
using System.Collections.Concurrent;
using Hangfire;
using Microsoft.Owin;

namespace Hangfire.JobDomains
{
    public static class GlobalConfigurationExtension
    {

        /// <summary>
        /// 任务域服务
        /// </summary>
        /// <param name="config">全局配置</param>
        /// <param name="path">插件路径</param>
        public static void UseDomains(this IGlobalConfiguration config, string path)
        {
            if (string.IsNullOrEmpty(path)) return;
            JobDomainManager.InitBoard(path);
            JobDomainManager.InitRoute();
        }
      
    }
   
}

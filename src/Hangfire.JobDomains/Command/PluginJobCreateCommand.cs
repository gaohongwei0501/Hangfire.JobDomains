using Common.Logging;
using CronExpressionDescriptor;
using Hangfire.PluginPackets.Dynamic;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Storage;
using Hangfire.States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Command
{
    public class PluginJobCreateCommand
    {
        static ILog loger = LogManager.GetLogger<PluginJobCreateCommand>();

        public static async Task Schedule(PluginParamer paramer)
        {
            if (IsPeriod(paramer.JobPeriod) == false) throw (new Exception("任务周期不能被识别"));
            AddJob<添加插件计划任务>(paramer);
        }

        public static void Delay(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            AddJob<添加插件排期任务>(paramer);
        }

        public static void Immediately(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            AddJob<添加插件即时任务>(paramer);

        }

        public static void Test(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            AddJob<添加插件测试任务>(paramer);
        }

        static void AddJob<T>(PluginParamer paramer) where T : JobCreateService, new()
        {
            var type = TypeFactory.CreateInheritType<DomainJobExecute>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
            hangFireClient.Create<T>(service => service.Create(paramer), state);
        }
    
        /// <summary>
        /// 判断 JobPeriod 格式是否符合要求
        /// </summary>
        static bool IsPeriod(string JobPeriod)
        {
            try
            {
                ExpressionDescriptor.GetDescription(JobPeriod);
                return true;
            }
            catch
            {
                return false;
            }
        }
       
    }
  
}

using Common.Logging;
using CronExpressionDescriptor;
using Hangfire.PluginPackets.Dynamic;
using Hangfire.PluginPackets.Interface;
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
    public class CreateJobCommand
    {
        static ILog loger = LogManager.GetLogger<CreateJobCommand>();

        static ConcurrentDictionary<string, Action<PluginParamer>> DynamicInvokes = new ConcurrentDictionary<string, Action<PluginParamer>>();

        public static  void Schedule(PluginParamer paramer)
        {
            if (IsPeriod(paramer.JobPeriod) == false) throw (new Exception("任务周期不能被识别"));
            var key = $"{paramer.PluginName}_{paramer.JobTitle}_Period";
            AddJob(key,paramer, "GetPeriodService");

            PlanCreate.AddNewPlan(paramer);

        }

        public static void Delay(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{paramer.PluginName}_{paramer.JobTitle}_Schedule";
            AddJob(key, paramer, "GetScheduleService");
        }

        public static void Immediately(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{paramer.PluginName}_{paramer.JobTitle}_Immediately";
            AddJob(key, paramer, "GetEnqueuedService");
        }

        public static void Test(PluginParamer paramer)
        {
            if (paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{paramer.PluginName}_{paramer.JobTitle}_Test";
            AddJob(key, paramer, "GetTestService");
        }


        static void AddJob(string key, PluginParamer paramer, string method)
        {
            var type = TypeFactory.CreateType<JobExecute>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(type, method));
            invoke(paramer);
        }

        static Action<PluginParamer> CreateBuilderInvoke(Type type, string invokeName)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(Action<PluginParamer>), new Type[] { }, true);
            ILGenerator IL = dynamicMethod.GetILGenerator();

            var genericTypeOfCts = typeof(JobCreate<>).MakeGenericType(type);
            var con = genericTypeOfCts.GetConstructor(new Type[] { });
            var method = genericTypeOfCts.GetMethod(invokeName);

            IL.Emit(OpCodes.Newobj, con);
            IL.Emit(OpCodes.Callvirt, method);
            IL.Emit(OpCodes.Ret);

            var fetch = (Func<Action<PluginParamer>>)dynamicMethod.CreateDelegate(typeof(Func<Action<PluginParamer>>));
            return fetch();
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

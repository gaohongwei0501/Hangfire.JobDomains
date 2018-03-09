using Common.Logging;
using CronExpressionDescriptor;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage
{
    
    public class DynamicService
    {

        static ILog loger = LogManager.GetLogger<DynamicService>();

        static ConcurrentDictionary<string, Action<PluginParamer>> DynamicInvokes = new ConcurrentDictionary<string, Action<PluginParamer>>();

        Type InvokeType { get; set; }

        PluginParamer Paramer { get; set; }

        public DynamicService(PluginParamer paramer)
        {
            InvokeType = DynamicFactory.GetType<DynamicBaseClass>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            Paramer = paramer;
        }

        public void TestDispatch()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobTitle}_Test";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetTestService"));
            invoke(Paramer);
        }

        public void ImmediatelyDispatch()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobTitle}_Immediately";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetEnqueuedService"));
            invoke(Paramer);
        }

        public void ScheduleDispatch()
        {
            if (Paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{Paramer.PluginName}_{Paramer.JobTitle}_Schedule";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetScheduleService"));
            invoke(Paramer);
        }

        public void PeriodDispatch()
        {
            if (IsPeriod(Paramer.JobPeriod) == false) throw (new Exception("任务周期不能被识别"));
            var key = $"{Paramer.PluginName}_{Paramer.JobTitle}_Period";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetPeriodService"));
            invoke(Paramer);
        }

        static Action<PluginParamer> CreateBuilderInvoke(Type type, string invokeName)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(Action<PluginParamer>), new Type[] { }, true);
            ILGenerator IL = dynamicMethod.GetILGenerator();

            var genericTypeOfCts = typeof(DynamicClassExtension<>).MakeGenericType(type);
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

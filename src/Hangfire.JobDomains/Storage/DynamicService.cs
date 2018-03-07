using Common.Logging;
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

        static ConcurrentDictionary<string, Action<JobParamer>> DynamicInvokes = new ConcurrentDictionary<string, Action<JobParamer>>();

        Type InvokeType { get; set; }

        JobParamer Paramer { get; set; }

        public DynamicService(JobParamer paramer)
        {
            InvokeType = DynamicFactory.GetType<DynamicBaseClass>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            Paramer = paramer;
        }

        public void TestDispatch()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Test";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetTestService"));
            invoke(Paramer);
        }

        public void ImmediatelyDispatch()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Immediately";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetEnqueuedService"));
            invoke(Paramer);
        }

        public void ScheduleDispatch()
        {
            if (Paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Schedule";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetScheduleService"));
            invoke(Paramer);
        }

        public void PeriodDispatch()
        {
            if (Paramer.IsPeriod == false) throw (new Exception("任务周期不能被识别"));
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Period";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetPeriodService"));
            invoke(Paramer);
        }

        static Action<JobParamer> CreateBuilderInvoke(Type type, string invokeName)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(Action<JobParamer>), new Type[] { }, true);
            ILGenerator IL = dynamicMethod.GetILGenerator();

            var genericTypeOfCts = typeof(DynamicClassExtension<>).MakeGenericType(type);
            var con = genericTypeOfCts.GetConstructor(new Type[] { });
            var method = genericTypeOfCts.GetMethod(invokeName);

            IL.Emit(OpCodes.Newobj, con);
            IL.Emit(OpCodes.Callvirt, method);
            IL.Emit(OpCodes.Ret);

            var fetch = (Func<Action<JobParamer>>)dynamicMethod.CreateDelegate(typeof(Func<Action<JobParamer>>));
            return fetch();
        }
       
    }


}

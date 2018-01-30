using Common.Logging;
using Hangfire.JobDomains.Models;
using Hangfire.JobDomains.Server;
using Hangfire.States;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage
{

    public class DynamicDispatch
    {

        static ILog loger = LogManager.GetLogger<DynamicDispatch>();

        static ConcurrentDictionary<string, Action<JobParamer>> DynamicInvokes = new ConcurrentDictionary<string, Action<JobParamer>>();

        Type InvokeType { get; set; }

        JobParamer Paramer { get; set; }

        public DynamicDispatch(JobParamer paramer)
        {
            InvokeType = DynamicFactory.GetType<DynamicBaseService>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            Paramer = paramer;
        }

        public void TestInvoke()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Test";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetTestInvoke"));
            invoke(Paramer);
        }

        public void ImmediatelyInvoke()
        {
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Immediately";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetEnqueuedInvoke"));
            invoke(Paramer);
        }

        public void ScheduleInvoke()
        {
            if (Paramer.JobDelay.Minutes < 0) throw (new Exception("任务启动时间设置失败"));
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Schedule";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetScheduleInvoke"));
            invoke(Paramer);
        }

        public void PeriodInvoke()
        {
            if (Paramer.IsPeriod == false) throw (new Exception("任务周期不能被识别"));
            var key = $"{Paramer.PluginName}_{Paramer.JobName}_Period";
            var invoke = DynamicInvokes.GetOrAdd(key, k => CreateBuilderInvoke(InvokeType, "GetPeriodInvoke"));
            invoke(Paramer);
        }

        static Action<JobParamer> CreateBuilderInvoke(Type type, string invokeName)
        {
            DynamicMethod dynamicMethod = new DynamicMethod(string.Empty, typeof(Action<JobParamer>), new Type[] { }, true);
            ILGenerator IL = dynamicMethod.GetILGenerator();

            var genericTypeOfCts = typeof(DynamicBaseClass<>).MakeGenericType(type);
            var con = genericTypeOfCts.GetConstructor(new Type[] { });
            var method = genericTypeOfCts.GetMethod(invokeName);

            IL.Emit(OpCodes.Newobj, con);
            IL.Emit(OpCodes.Callvirt, method);
            IL.Emit(OpCodes.Ret);

            var fetch = (Func<Action<JobParamer>>)dynamicMethod.CreateDelegate(typeof(Func<Action<JobParamer>>));
            return fetch();
        }
       
    }


    public class DynamicBaseClass<T> where T : DynamicBaseService
    {

        public Action<JobParamer> GetTestInvoke()
        {
            return paramer =>
            {
                IBackgroundJobClient hangFireClient = new BackgroundJobClient();
                EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
                hangFireClient.Create<T>(service => service.TestInvoke(paramer), state);
            };
        }

        public Action<JobParamer> GetScheduleInvoke()
        {
            return paramer =>
            {
                BackgroundJob.Schedule<T>(service => service.Enqueued(paramer), paramer.JobDelay);
            };
        }

        public Action<JobParamer> GetEnqueuedInvoke()
        {
            return paramer =>
             {
                 IBackgroundJobClient hangFireClient = new BackgroundJobClient();
                 EnqueuedState state = new Hangfire.States.EnqueuedState(paramer.QueueName);
                 hangFireClient.Create<T>(service => service.Invoke(paramer), state);
             };
        }

        public Action<JobParamer> GetPeriodInvoke()
        {
            return paramer =>
            {
                RecurringJob.AddOrUpdate<T>(paramer.JobTitle, service => service.Invoke(paramer), paramer.JobPeriod, queue: paramer.QueueName);
            };
        }

    }






}

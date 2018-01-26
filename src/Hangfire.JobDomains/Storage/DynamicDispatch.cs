using Hangfire.Common;
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

        public static void TestInvoke(string pluginName, string jobDesc, string assembly, string job, object[] paramers, string queue)
        {
            var invoke = DynamicInvokeBuilder.TestInvoke<DynamicDispatch>(pluginName, job);
            invoke(queue, pluginName, assembly, job, paramers);
        }

        public static void ImmediatelyInvoke(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            var invoke = DynamicInvokeBuilder.ImmediatelyInvoke<DynamicDispatch>(pluginName, job);
            invoke(queue, pluginName, assembly, job, paramers);
        }


        public static void ScheduleInvoke(TimeSpan delay, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            var invoke = DynamicInvokeBuilder.ScheduleInvoke<DynamicDispatch>(pluginName, job);
            invoke(queue, pluginName, assembly, job, paramers);
        }
      

        public static void PeriodInvoke(string period, string queue, string jobSign, string pluginName, string assembly, string job, object[] paramers)
        {
            var invoke = DynamicInvokeBuilder.PeriodInvoke<DynamicDispatch>(pluginName, job);
            invoke(queue, pluginName, assembly, job, paramers);
        }
    
    }

    /// <summary>
    /// 动态填写实体类的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicInvokeBuilder
    {

        private static ConcurrentDictionary<string, Action<string, string, string, string, object[]>> DynamicImmediatelyInvokes = new ConcurrentDictionary<string, Action<string, string, string, string, object[]>>();
        private static ConcurrentDictionary<string, Action> DynamicScheduleInvokes = new ConcurrentDictionary<string, Action>();
        private static ConcurrentDictionary<string, Action> DynamicPeriodInvokes = new ConcurrentDictionary<string, Action>();

        private static readonly MethodInfo CreateMethod = typeof(IBackgroundJobClient).GetMethod("Create", new Type[] { typeof(Job),typeof(IState) });

        #region TestInvoke

        public static Action<string, string, string, string, object[]> TestInvoke<T>(string pluginName, string jobName)
        {
            return DynamicImmediatelyInvokes.GetOrAdd($"{pluginName}_{jobName}_Test", key => CreateBuilderTestInvoke<T>(key));
        }

        static Action<string, string, string, string, object[]> CreateBuilderTestInvoke<T>(string name)
        {
            DynamicMethod method = new DynamicMethod(name, typeof(T), new Type[] {   }, typeof(T), true);
            ILGenerator IL = method.GetILGenerator();

            var invoke = IL.DeclareLocal(typeof(Hangfire.JobDomains.Server.JobInvoke));
            var state = IL.DeclareLocal(typeof(Hangfire.States.EnqueuedState));

            //IL.Emit(OpCodes.Ldarg_0);
            //IL.Emit(OpCodes.Callvirt, NewRow);
            //IL.Emit(OpCodes.Stloc_0);





            IL.Emit(OpCodes.Ret);

            return (Action<string, string, string, string, object[]>)method.CreateDelegate(typeof(Action<string, string, string, string, object[]>));
        }

        #endregion

        #region ImmediatelyInvoke

        public static Action<string, string, string, string, object[]> ImmediatelyInvoke<T>(string pluginName, string jobName)
        {
            return DynamicImmediatelyInvokes.GetOrAdd($"{pluginName}_{jobName}_Immediately", key => CreateBuilderImmediatelyInvoke<T>());
        }

        static Action<string, string, string, string, object[]> CreateBuilderImmediatelyInvoke<T>()
        {

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);



            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            return (Action<string, string, string, string, object[]>)method.CreateDelegate(typeof(Action<string, string, string, string, object[]>));
        }

        #endregion

        #region ScheduleInvoke

        public static Action<string, string, string, string, object[]> ScheduleInvoke<T>(string pluginName, string jobName)
        {
            return DynamicImmediatelyInvokes.GetOrAdd($"{pluginName}_{jobName}_Test", key => CreateBuilderScheduleInvoke<T>());
        }

        static Action<string, string, string, string, object[]> CreateBuilderScheduleInvoke<T>()
        {

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);



            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            return (Action<string, string, string, string, object[]>)method.CreateDelegate(typeof(Action<string, string, string, string, object[]>));
        }

        #endregion

        #region PeriodInvoke

        public static Action<string, string, string, string, object[]> PeriodInvoke<T>(string pluginName, string jobName)
        {
            return DynamicImmediatelyInvokes.GetOrAdd($"{pluginName}_{jobName}_Test", key => CreateBuilderPeriodInvoke<T>());
        }

        static Action<string, string, string, string, object[]> CreateBuilderPeriodInvoke<T>()
        {

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);



            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            return (Action<string, string, string, string, object[]>)method.CreateDelegate(typeof(Action<string, string, string, string, object[]>));
        }

        #endregion


        static Action CreateBuilder<T>()
        {

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            return (Action)method.CreateDelegate(typeof(Action));
        }
    }

   

    internal class TestJobInvoke
    {

        public void ScheduleInvoke(TimeSpan delay, string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            BackgroundJob.Schedule(() => ImmediatelyEnqueued(queue, pluginName, assembly, job, paramers), delay);
        }

        void ImmediatelyEnqueued(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create(() => _JobInvoke.Invoke(pluginName, assembly, job, paramers), state);
        }

        public void RecurringInvoke(string period, string queue, string jobSign, string pluginName, string assembly, string job, object[] paramers)
        {
            RecurringJob.AddOrUpdate(jobSign, () => _JobInvoke.Invoke(pluginName, assembly, job, paramers), period, queue: queue);
        }


        public void Test(string queue, string pluginName, string assembly, string job, object[] paramers)
        {
            IBackgroundJobClient hangFireClient = new BackgroundJobClient();
            EnqueuedState state = new Hangfire.States.EnqueuedState(queue);
            hangFireClient.Create(() => _JobInvoke.TestInvoke(pluginName, assembly, job, paramers), state);
        }

    }
}

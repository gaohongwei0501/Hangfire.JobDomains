using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    public abstract class JobCreateService
    {
        static ConcurrentDictionary<Type, Action<PluginParamer>> DynamicInvokes = new ConcurrentDictionary<Type, Action<PluginParamer>>();

        public abstract void Create(PluginParamer paramer);

        protected static void CreateJob<T>(PluginParamer paramer, string method )where T:JobExecute,new()
        {
            var type = TypeFactory.CreateInheritType<T>(paramer.PluginName, paramer.AssemblyName, paramer.JobName, paramer.JobTitle);
            var invoke = DynamicInvokes.GetOrAdd(type, k => CreateBuilderInvoke(type, method));
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

    }
}

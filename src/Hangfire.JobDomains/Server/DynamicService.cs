using Hangfire.JobDomains.Interface;
using Hangfire.JobDomains.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Server
{
    public class DynamicService
    {




        public static bool TestInvoke(string pluginName, string assembly, string job, object[] paramers)
        {
            return DomainInvoke<bool>(pluginName, assembly, job, paramers, PrefabricationActivator.Test, domain => (bool)domain.GetData("result"));
        }


        static T DomainInvoke<T>(string pluginName, string assembly, string job, object[] paramers, Action act, Func<AppDomain, T> GetResult)
        {
            AppDomain Domain = null;
            try
            {
                var server = StorageService.Provider.GetServer(null, PluginServiceManager.ServerName);
                var path = $"{ server.PlugPath }//{ pluginName }";
                AppDomainSetup setup = new AppDomainSetup
                {
                    ApplicationBase = Path.GetDirectoryName(path),
                    ConfigurationFile = $"{path}\\App.config",
                    PrivateBinPath = path,
                    DisallowApplicationBaseProbing = false,
                    DisallowBindingRedirects = false
                };
                Domain = AppDomain.CreateDomain($"Plugin AppDomain { Guid.NewGuid() } ", null, setup);
                if (Directory.Exists(path) == false) throw (new Exception("此服务器不支持该插件"));
                var args = new CrossDomainData { PluginDir = path, assemblyName = assembly, typeName = job, paramers = paramers };
                Domain.SetData("args", args);
                Domain.DoCallBack(new CrossAppDomainDelegate(act));
                return GetResult(Domain);
            }
            finally
            {
                if (Domain != null) AppDomain.Unload(Domain);
            }
        }

        public static void CreateAssembly()
        {
            CreateAssembly("DynamicAssembly", "", "", "", new List<Action<TypeBuilder>> {
                  tb=> CreateJobInvokeMethod(tb,"数据同步1"),
                  tb=> CreateJobInvokeMethod(tb,"数据同步2"),
            });
        }


        static void CreateJobInvokeMethod(TypeBuilder typeBuilder, string name)
        {
            MethodBuilder theMethod = typeBuilder.DefineMethod(name, MethodAttributes.Public, null, new Type[] { });
            ILGenerator IL = theMethod.GetILGenerator();




            IL.Emit(OpCodes.Ret);
        }


        static void CreateAssembly(string path, string assemblyName, string moduleName, string typeName, List<Action<TypeBuilder>> Methods)
        {
            AssemblyName assembly = new AssemblyName(assemblyName);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule(moduleName, $"{assemblyName}.dll");
            TypeBuilder typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Public);

            foreach (var createMethod in Methods)
            {
                createMethod(typeBuilder);
            }

            Type t = typeBuilder.CreateType();
            assemblyBuilder.Save($"{path}/{assemblyName}.dll");
        }

    }


    /// <summary>
    /// 动态填写实体类的值
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class DynamicEntityBuilder<T>
    {
        private static readonly MethodInfo getValueMethod = typeof(IDataRecord).GetMethod("get_Item", new Type[] { typeof(int) });
        private static readonly MethodInfo isDBNullMethod = typeof(IDataRecord).GetMethod("IsDBNull", new Type[] { typeof(int) });
        private delegate T Load(IDataRecord dataRecord);
        private Load handler;

        private DynamicEntityBuilder() { }

        public T Build(IDataRecord dataRecord)
        {
            return handler(dataRecord);
        }

        public static DynamicEntityBuilder<T> CreateBuilder(IDataRecord dataRecord)
        {
            DynamicEntityBuilder<T> dynamicBuilder = new DynamicEntityBuilder<T>();

            DynamicMethod method = new DynamicMethod("DynamicCreate", typeof(T), new Type[] { typeof(IDataRecord) }, typeof(T), true);
            ILGenerator generator = method.GetILGenerator();

            LocalBuilder result = generator.DeclareLocal(typeof(T));
            generator.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes));
            generator.Emit(OpCodes.Stloc, result);

            for (int i = 0; i < dataRecord.FieldCount; i++)
            {
                PropertyInfo propertyInfo = typeof(T).GetProperty(dataRecord.GetName(i));
                Label endIfLabel = generator.DefineLabel();

                if (propertyInfo != null && propertyInfo.GetSetMethod() != null)
                {
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, isDBNullMethod);
                    generator.Emit(OpCodes.Brtrue, endIfLabel);

                    generator.Emit(OpCodes.Ldloc, result);
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldc_I4, i);
                    generator.Emit(OpCodes.Callvirt, getValueMethod);
                    generator.Emit(OpCodes.Unbox_Any, dataRecord.GetFieldType(i));
                    generator.Emit(OpCodes.Callvirt, propertyInfo.GetSetMethod());

                    generator.MarkLabel(endIfLabel);
                }
            }

            generator.Emit(OpCodes.Ldloc, result);
            generator.Emit(OpCodes.Ret);

            dynamicBuilder.handler = (Load)method.CreateDelegate(typeof(Load));
            return dynamicBuilder;
        }
    }

}

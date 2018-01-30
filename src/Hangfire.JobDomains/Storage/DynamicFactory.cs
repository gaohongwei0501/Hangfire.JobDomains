using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage
{
    public class DynamicFactory
    {

        private static ConcurrentDictionary<string, Type> DynamicTypes = new ConcurrentDictionary<string, Type>();

        public static Type GetType<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {
            assemblyName = $"{ plugName }.{ assemblyName }.{ jobName }";
            var className = jobTitle.Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var key = $"{ plugName }.{ assemblyName }.{ className }";

            var dynamicType = DynamicTypes.GetOrAdd(key, k =>
            {
                var assembly = new AssemblyName(assemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, $"{assemblyName}.dll");
                //定义公开,继承Object,无接口的类
                var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

                var _type = typeBuilder.CreateType();
               // assemblyBuilder.Save($"Dynamic.{plugName}.{assembly}.dll");
                return _type;
            });
            return dynamicType;
        }

        public static void Create<T>(string plugName, string assemblyName, string jobName, string jobTitle,string path) where T : class
        {
            assemblyName = $"{ plugName }.{ assemblyName }.{ jobName }";
            var className = jobTitle.Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            //var key = $"{ plugName }.{ assemblyName }.{ className }";

            //var dynamicType = DynamicTypes.GetOrAdd(key, k =>
            //{
            //    var assembly = new AssemblyName(assemblyName);
            //    var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.RunAndSave, path);
            //    var moduleBuilder = assemblyBuilder.DefineDynamicModule("MainModule", $"{assemblyName}.dll");
            //    //定义公开,继承Object,无接口的类
            //    var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

            //    var _type = typeBuilder.CreateType();
            //    assemblyBuilder.Save($"{plugName}.{assembly}.dll");
            //    return _type;
            //});

            var assembly = new AssemblyName(assemblyName);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Save, path);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, $"{assemblyName}.dll");
            //定义公开,继承Object,无接口的类
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

            var _type = typeBuilder.CreateType();
            assemblyBuilder.Save($"{assembly}.dll");
        }



        public static T CreateInstance<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T:class
        {
            assemblyName = $"{ plugName }.{ assemblyName }.{ jobName }";
            var className = jobTitle.Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var key = $"{ plugName }.{ assemblyName }.{ className }";

            var dynamicType = DynamicTypes.GetOrAdd(key, k =>
            {
                var assembly = new AssemblyName(assemblyName);
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Run);
                var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, $"{assemblyName}.dll");
                //定义公开,继承Object,无接口的类
                var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

                var _type = typeBuilder.CreateType();
               // assemblyBuilder.Save($"Dynamic\\{plugName}\\{assembly}.dll");
                return _type;
            });

            return Activator.CreateInstance(dynamicType) as T;
        }
    }

}

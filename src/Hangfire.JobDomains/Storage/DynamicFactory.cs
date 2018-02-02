using System;
using System.IO;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace Hangfire.PluginPackets.Storage
{
    public class DynamicFactory
    {


        public static string DynamicPath { get; }= $"{ AppDomain.CurrentDomain.BaseDirectory }Dynamic";

        static DynamicFactory()
        {
            if (string.IsNullOrEmpty(DynamicPath)) return;
            if (Directory.Exists(DynamicPath)) Directory.Delete(DynamicPath, true);
            Thread.Sleep(1000);
            Directory.CreateDirectory(DynamicPath);
        }

        public static Type GetType<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {
            var  assembly = $"{ plugName }.{ assemblyName }.{ jobName }";
            var className = jobTitle.Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var key = $"{ plugName }.{ assembly }.{ className }";
            var file = $"{DynamicPath}\\{assembly}.dll";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains(assembly));
            if (dynamicAssembly == null && File.Exists(file) == false) dynamicAssembly = Create<T>(plugName, assemblyName, jobName, jobTitle);
            var dynamicType = dynamicAssembly.GetType(className);
            return dynamicType;
        }

        public static Assembly Create<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {
            assemblyName = $"{ plugName }.{ assemblyName }.{ jobName }";
            var className = jobTitle.Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");

            var assembly = new AssemblyName(assemblyName);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Save, DynamicPath);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, $"{assemblyName}.dll");
            //定义公开,继承Object,无接口的类
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

            var _type = typeBuilder.CreateType();
            assemblyBuilder.Save($"{assembly}.dll");

            var file = $"{DynamicPath}\\{assemblyName}.dll";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.SingleOrDefault(s => s.FullName.Contains(assemblyName));
            return dynamicAssembly;
        }


        static byte[] loadFile(string filename)
        {
            FileStream fs = new FileStream(filename, FileMode.Open);
            byte[] buffer = new byte[(int)fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();

            return buffer;
        }
    }

}

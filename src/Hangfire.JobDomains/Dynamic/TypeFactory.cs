using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Dynamic
{
    public class TypeFactory
    {

        public static string DynamicPath { get; } = $"{ AppDomain.CurrentDomain.BaseDirectory }Dynamic";

        //static DynamicFactory()
        //{
        //    if (string.IsNullOrEmpty(DynamicPath)) return;
        //    if (Directory.Exists(DynamicPath)) Directory.Delete(DynamicPath, true);
        //    Thread.Sleep(1000);
        //    Directory.CreateDirectory(DynamicPath);
        //}

        public static Type CreateType<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {

            var className = jobTitle.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var assembly = $"{ plugName }.{ assemblyName }.{ jobName }.{ className }_Assembly";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains($"{assembly},") );
            if (dynamicAssembly == null) CreateType<T>(assembly, className);

            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains($"{assembly},"));

            var dynamicType = dynamicAssembly.GetType(className, throwOnError: false, ignoreCase: true);
            var type = System.Type.GetType(dynamicType.AssemblyQualifiedName, throwOnError: false, ignoreCase: true);

            return type;
        }

        static void CreateType<T>(string assemblyName, string className) where T : class
        {
            var assembly = new AssemblyName(assemblyName);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Save, DynamicPath);

            string dynamicTypeName = Assembly.CreateQualifiedName(typeof(T).AssemblyQualifiedName, assemblyName);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(dynamicTypeName, $"{assemblyName}.dll");
            //定义公开,继承Object,无接口的类
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

            var _type = typeBuilder.CreateType();
            assemblyBuilder.Save($"{assembly}.dll");
        }


        public static Type GetType<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {
            var className = jobTitle.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var assembly = $"{ plugName }.{ assemblyName }.{ jobName }.{ className }";

            var key = $"{ plugName }.{ assembly }.{ className }";
            var file = $"{DynamicPath}\\{assembly}.dll";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains($"{assembly},"));
            if (dynamicAssembly == null && File.Exists(file) == false) dynamicAssembly = Create<T>(plugName, assemblyName, jobName, jobTitle);

            var dynamicType = dynamicAssembly.GetType(className);
            return dynamicType;
        }

        public static Assembly Create<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {
            var className = jobTitle.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            assemblyName = $"{ plugName }.{ assemblyName }.{ jobName }.{ className }";

            var assembly = new AssemblyName(assemblyName);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assembly, AssemblyBuilderAccess.Save, DynamicPath);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, $"{assemblyName}.dll");
            //定义公开,继承Object,无接口的类
            var typeBuilder = moduleBuilder.DefineType(className, TypeAttributes.Public | TypeAttributes.Serializable, typeof(T), new Type[0]);

            var _type = typeBuilder.CreateType();
            assemblyBuilder.Save($"{assembly}.dll");

            var file = $"{DynamicPath}\\{assemblyName}.dll";
            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.SingleOrDefault(s => s.FullName.Contains($"{assemblyName},") );

            AppDomain.CurrentDomain.SetupInformation.PrivateBinPath = TypeFactory.DynamicPath;

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

using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace Hangfire.PluginPackets.Dynamic
{
    public class TypeFactory
    {

        public static string DynamicPath { get; } = $"{ AppDomain.CurrentDomain.BaseDirectory }Dynamic";

        public static Type CreateInheritType<T>(string plugName, string assemblyName, string jobName, string jobTitle) where T : class
        {

            var className = jobTitle.Replace(" ", "_").Replace("-", "_").Replace(".", "_").Replace("\"", "_").Replace("'", "_");
            var assembly = $"{ plugName }.{ assemblyName }.{ jobName }.{ className }_Assembly";

            var assemblies = AppDomain.CurrentDomain.GetAssemblies();
            var dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains($"{assembly},") );
            if (dynamicAssembly == null) CreateInheritType<T>(assembly, className);

            assemblies = AppDomain.CurrentDomain.GetAssemblies();
            dynamicAssembly = assemblies.FirstOrDefault(s => s.FullName.Contains($"{assembly},"));

            var dynamicType = dynamicAssembly.GetType(className, throwOnError: false, ignoreCase: true);
            var type = System.Type.GetType(dynamicType.AssemblyQualifiedName, throwOnError: false, ignoreCase: true);

            return type;
        }

        static void CreateInheritType<T>(string assemblyName, string className) where T : class
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

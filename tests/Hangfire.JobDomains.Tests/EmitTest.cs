using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Tests
{
    [TestClass]
    public class EmitTest
    {
        [TestMethod]
        private void EmitHelperTest()
        {
            var watch = Stopwatch.StartNew();
           // string s = null;
            var st2 = new Student() { Name = "aaaaaa" };
            for (var i = 0; i < 5000000; i++)
                st2.Name = "sdasdas11";

            Console.WriteLine("native:{0}", watch.ElapsedTicks);

            var watch3 = Stopwatch.StartNew();
            //s = null;
            dynamic st1 = new Student() { Name = "sdasdas" };
            for (var i = 0; i < 5000000; i++)
                st1.Name = "sdasdas22";

            Console.WriteLine("dynamic:{0}", watch3.ElapsedTicks);

            EmitHelper emit = new EmitHelper();
            emit.CreateType("DynamicAssembly", "Student2");
            emit.CreateProperty("Name", typeof(string));
            //emit.Save();
            object ee = emit.CreateInstance();
            emit.Set(ee, "Name", "sdasdas");
            var watch2 = Stopwatch.StartNew();
            for (var i = 0; i < 5000000; i++)
                emit.Set(ee, "Name", "sdasdas22");
            Console.WriteLine("emitType:{0}", watch2.ElapsedTicks);
        }


    }

    public class Student {
        public string Name { get; set; }
    }
    //测试

    public class EmitHelper
    {
        private TypeBuilder _typeBuilder = null;
        private AssemblyBuilder _assemblyBuilder = null;
        public Type ThisType { get; set; }

        public delegate void SetPropertyValue(object obj, object value);
        public delegate object GetPropertyValue(object obj);
        private Dictionary<string, SetPropertyValue> _dicSetProperty = new Dictionary<string, SetPropertyValue>();
        private Dictionary<string, GetPropertyValue> _dicGetProperty = new Dictionary<string, GetPropertyValue>();

        public void CreateType(string assembly, string type)
        {
            AssemblyName DemoName = new AssemblyName(assembly);
            _assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(DemoName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder mb = _assemblyBuilder.DefineDynamicModule(DemoName.Name, DemoName.Name + ".dll");
            _typeBuilder = mb.DefineType(type, TypeAttributes.Public);
        }

        public void CreateProperty(string propertyName, Type propertyType)
        {
            TypeBuilder tb = _typeBuilder;
            var field = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);
            var getMethod = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public, propertyType, null);
            var setMethod = tb.DefineMethod("set_" + propertyName, MethodAttributes.Public, null, new Type[] { propertyType });
            var ilGet = getMethod.GetILGenerator();
            ilGet.Emit(OpCodes.Ldarg_0);
            ilGet.Emit(OpCodes.Ldfld, field);
            ilGet.Emit(OpCodes.Ret);

            var ilSet = setMethod.GetILGenerator();
            ilSet.Emit(OpCodes.Ldarg_0);
            ilSet.Emit(OpCodes.Ldarg_1);
            ilSet.Emit(OpCodes.Stfld, field);
            ilSet.Emit(OpCodes.Ret);

            var property = tb.DefineProperty(propertyName, PropertyAttributes.None, propertyType, null);
            property.SetGetMethod(getMethod);
            property.SetSetMethod(setMethod);

            var getMethod2 = tb.DefineMethod("get2_" + propertyName, MethodAttributes.Public, typeof(object), null);
            var ilGet2 = getMethod2.GetILGenerator();
            ilGet2.Emit(OpCodes.Ldarg_0);
            ilGet2.Emit(OpCodes.Ldfld, field);
            ilGet2.Emit(OpCodes.Ret);

            var setMethod2 = tb.DefineMethod("set2_" + propertyName, MethodAttributes.Public, null, new Type[] { typeof(object) });
            var ilSet2 = setMethod2.GetILGenerator();
            ilSet2.Emit(OpCodes.Ldarg_0);
            ilSet2.Emit(OpCodes.Ldarg_1);
            if (propertyType.IsValueType)
            {
                ilSet2.Emit(OpCodes.Unbox_Any, propertyType);// 如果是值类型，拆箱 string = (string)object;
            }
            else
            {
                ilSet2.Emit(OpCodes.Castclass, propertyType);// 如果是引用类型，转换 Class = object as Class
            }
            ilSet2.Emit(OpCodes.Stfld, field);
            ilSet2.Emit(OpCodes.Ret);

            Save();

            var dm = new DynamicMethod(name: "EmittedGetter", returnType: typeof(object), parameterTypes: new[] { typeof(object) }, owner: ThisType);
            var type = ThisType;
            var prop = type.GetMethod("get2_" + propertyName);
            var il2 = dm.GetILGenerator();
            il2.Emit(OpCodes.Ldarg_0);
            il2.Emit(OpCodes.Call, prop);
            il2.Emit(OpCodes.Ret);
            _dicGetProperty[propertyName] = dm.CreateDelegate(typeof(GetPropertyValue)) as GetPropertyValue;

            var callMethod = ThisType.GetMethod("set2_" + propertyName, BindingFlags.Instance | BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.NonPublic);
            var para = callMethod.GetParameters()[0];
            // 创建动态函数
            DynamicMethod method = new DynamicMethod("EmittedSetter", null, new Type[] { typeof(object), typeof(object) }, ThisType);
            // 获取动态函数的 IL 生成器
            var il = method.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            il.Emit(OpCodes.Ldarg_1);
            il.EmitCall(OpCodes.Callvirt, callMethod, null);//调用函数
            il.Emit(OpCodes.Ret);   // 返回
            _dicSetProperty[propertyName] = method.CreateDelegate(typeof(SetPropertyValue)) as SetPropertyValue;
        }

        public Type Save()
        {
            ThisType = _typeBuilder.CreateType();
            return ThisType;
        }

        public object CreateInstance()
        {
            return Activator.CreateInstance(ThisType);
        }

        public void Set(object obj, string propertyName, object value)
        {
            _dicSetProperty[propertyName](obj, value);
        }

        public object Get(object obj, string propertyName)
        {
            return _dicGetProperty[propertyName](obj);
        }
    }
}

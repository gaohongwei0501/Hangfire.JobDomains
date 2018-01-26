using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hangfire.JobDomains.Tests
{
    [TestClass]
    public class DynamicTaskTest
    {
        [TestMethod]
        public void TestMethod1()
        {
            CreateTestAssembly();
        }

        public static void CreateTestAssembly()
        {
            CreateAssembly(new List<Action<TypeBuilder>> {
               CreateHelloWorldMethod,
               CreateHelloWorldMethod,
            });
        }


        static void CreateHelloWorldMethod(TypeBuilder typeBuilder)
        {
            MethodBuilder theMethod = typeBuilder.DefineMethod("数据同步", MethodAttributes.Public, null, new Type[] { });
            ILGenerator IL = theMethod.GetILGenerator();

         

            IL.Emit(OpCodes.Ret);
        }


        static void CreateAssembly(List<Action<TypeBuilder>> Methods)
        {

            AssemblyName assemblyName = new AssemblyName("Study");
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("StudyModule", "StudyOpCodes.dll");
            TypeBuilder typeBuilder = moduleBuilder.DefineType("StudyOpCodes", TypeAttributes.Public);

            foreach (var createMethod in Methods)
            {
                createMethod(typeBuilder);
            }

            Type t = typeBuilder.CreateType();
            assemblyBuilder.Save("StudyOpCodes.dll");
        }

    }


}

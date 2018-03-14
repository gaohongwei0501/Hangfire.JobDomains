using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Hangfire.PluginPackets.Dynamic;
using Hangfire.PluginPackets.Interface;
using Hangfire.PluginPackets.Models;
using Hangfire.PluginPackets.Server;
using Hangfire.PluginPackets.Storage;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Hangfire.PluginPackets.Tests
{

    [TestClass]
    public class DynamicTaskTest
    {
        [TestMethod]
        public void CreateStudyOpCodes()
        {
            CreateTestAssembly();
        }

        public static void CreateTestAssembly()
        {
            CreateAssembly(new List<Action<TypeBuilder>> {
               CreateDynamicTestClassTestMethod,
            });
        }

        public class DynamicTestClass : JobExecute { }

        static void CreateDynamicTestClassTestMethod(TypeBuilder typeBuilder)
        {
            MethodBuilder theMethod = typeBuilder.DefineMethod("DynamicTestClassTest", MethodAttributes.Public,typeof(Action<PluginParamer>), new Type[] { });
            ILGenerator IL = theMethod.GetILGenerator();

            var genericTypeOfCts = typeof(JobCreate<>).MakeGenericType(typeof(DynamicTestClass));
            var con = genericTypeOfCts.GetConstructor(new Type[] { });
            var method = genericTypeOfCts.GetMethod("GetTestInvoke");

            IL.Emit(OpCodes.Newobj, con);
            IL.Emit(OpCodes.Callvirt, method);
            IL.Emit(OpCodes.Ret);
        }


        public void Test2() {

            const string assemblyName = "SampleAssembly";
            const string fieldName = "Items";
            const string typeName = "Sample";
            const string assemblyFileName = assemblyName + ".dll";

            var domain = AppDomain.CurrentDomain;
            var assemblyBuilder = domain.DefineDynamicAssembly(new AssemblyName(assemblyName), AssemblyBuilderAccess.RunAndSave);

            var moduleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName, assemblyFileName);
            var typeBuilder = moduleBuilder.DefineType(typeName, TypeAttributes.Class | TypeAttributes.Public);

            var typeOfCts = typeof(JobCreate<>);
            var genericTypeOfCts = typeOfCts.MakeGenericType(typeBuilder);

            var fieldBuilder = typeBuilder.DefineField(fieldName, genericTypeOfCts, FieldAttributes.Public);

            //first constructor Sample()
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, Type.EmptyTypes);
            var obsCtor1 = typeOfCts.GetConstructors().First(c => c.GetParameters().Length == 1);
            obsCtor1 = TypeBuilder.GetConstructor(genericTypeOfCts, obsCtor1); //hack to get close generic type ctor with typeBuilder as generic parameter

            var generator = ctorBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0); //load this for base type constructor
            generator.Emit(OpCodes.Call, typeof(object).GetConstructors().Single());

            generator.Emit(OpCodes.Ldarg_0); //load this for field setter

            generator.Emit(OpCodes.Ldarg_0); //load this for ObservableTestCollection constructor
            generator.Emit(OpCodes.Newobj, obsCtor1); //call ObservableTestCollection constructor, it will put point to new object in stack

            generator.Emit(OpCodes.Stfld, fieldBuilder); // store into Items
            generator.Emit(OpCodes.Ret); //return

            //second constructor Sample(IEnumerable<Sample> source)
            var ctorParam = typeof(IEnumerable<>).MakeGenericType(typeBuilder);
            ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.HasThis, new Type[] { ctorParam });
            obsCtor1 = typeOfCts.GetConstructors().First(c => c.GetParameters().Length == 2);
            obsCtor1 = TypeBuilder.GetConstructor(genericTypeOfCts, obsCtor1); //hack to get close generic type ctor with typeBuilder as generic parameter

            generator = ctorBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0); //load this for base type constructor
            generator.Emit(OpCodes.Call, typeof(object).GetConstructors().Single());

            generator.Emit(OpCodes.Ldarg_0); //load this for field setter

            generator.Emit(OpCodes.Ldarg_0); //load this for ObservableTestCollection constructor
            generator.Emit(OpCodes.Ldarg_1); //load IEnumerable for ObservableTestCollection constructor
            generator.Emit(OpCodes.Newobj, obsCtor1); //call ObservableTestCollection constructor, it will put point to new object in stack

            generator.Emit(OpCodes.Stfld, fieldBuilder); // store into Items
            generator.Emit(OpCodes.Ret); //return


            var type = typeBuilder.CreateType();
            var obj1 = Activator.CreateInstance(type);

            var parameter = Activator.CreateInstance(typeof(List<>).MakeGenericType(type));
            var obj2 = Activator.CreateInstance(type, parameter);
            assemblyBuilder.Save(assemblyFileName);
        }



        public void Test()
        {

            AppDomain myDomain = AppDomain.CurrentDomain;
            AssemblyName myAsmName = new AssemblyName("TypeBuilderGetFieldExample");
            AssemblyBuilder myAssembly = myDomain.DefineDynamicAssembly(myAsmName, AssemblyBuilderAccess.Save);
            ModuleBuilder myModule = myAssembly.DefineDynamicModule(myAsmName.Name, myAsmName.Name + ".exe");

            // Define the sample type.
            TypeBuilder myType = myModule.DefineType("Sample", TypeAttributes.Class | TypeAttributes.Public);

            // Add a type parameter, making the type generic.
            string[] typeParamNames = { "T" };
            GenericTypeParameterBuilder[] typeParams = myType.DefineGenericParameters(typeParamNames);

            // Define a default constructor. Normally it would 
            // not be necessary to define the default constructor,
            // but in this case it is needed for the call to
            // TypeBuilder.GetConstructor, which gets the default
            // constructor for the generic type constructed from 
            // Sample<T>, in the generic method GM<U>.
            ConstructorBuilder ctor = myType.DefineDefaultConstructor(
                MethodAttributes.PrivateScope | MethodAttributes.Public |
                MethodAttributes.HideBySig | MethodAttributes.SpecialName |
                MethodAttributes.RTSpecialName);

            // Add a field of type T, with the name Field.
            FieldBuilder myField = myType.DefineField("Field", typeParams[0], FieldAttributes.Public);

            // Add a method and make it generic, with a type 
            // parameter named U. Note how similar this is to 
            // the way Sample is turned into a generic type. The
            // method has no signature, because the type of its
            // only parameter is U, which is not yet defined.
            MethodBuilder genMethod = myType.DefineMethod("GM", MethodAttributes.Public | MethodAttributes.Static);
            string[] methodParamNames = { "U" };
            GenericTypeParameterBuilder[] methodParams = genMethod.DefineGenericParameters(methodParamNames);

            // Now add a signature for genMethod, specifying U
            // as the type of the parameter. There is no return value
            // and no custom modifiers.
            genMethod.SetSignature(null, null, null, new Type[] { methodParams[0] }, null, null);

            // Emit a method body for the generic method.
            ILGenerator ilg = genMethod.GetILGenerator();
            // Construct the type Sample<U> using MakeGenericType.
            Type SampleOfU = myType.MakeGenericType(methodParams[0]);
            // Create a local variable to store the instance of
            // Sample<U>.
            ilg.DeclareLocal(SampleOfU);
            // Call the default constructor. Note that it is 
            // necessary to have the default constructor for the
            // constructed generic type Sample<U>; use the 
            // TypeBuilder.GetConstructor method to obtain this 
            // constructor.
            ConstructorInfo ctorOfU = TypeBuilder.GetConstructor(SampleOfU, ctor);
            ilg.Emit(OpCodes.Newobj, ctorOfU);
            // Store the instance in the local variable; load it
            // again, and load the parameter of genMethod.
            ilg.Emit(OpCodes.Stloc_0);
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldarg_0);
            // In order to store the value in the field of the
            // instance of Sample<U>, it is necessary to have 
            // a FieldInfo representing the field of the 
            // constructed type. Use TypeBuilder.GetField to 
            // obtain this FieldInfo.
            FieldInfo FieldOfU = TypeBuilder.GetField(SampleOfU, myField);
            // Store the value in the field. 
            ilg.Emit(OpCodes.Stfld, FieldOfU);
            // Load the instance, load the field value, box it
            // (specifying the type of the type parameter, U), and
            // print it.
            ilg.Emit(OpCodes.Ldloc_0);
            ilg.Emit(OpCodes.Ldfld, FieldOfU);
            ilg.Emit(OpCodes.Box, methodParams[0]);
            MethodInfo writeLineObj =
                typeof(Console).GetMethod("WriteLine",
                    new Type[] { typeof(object) });
            ilg.EmitCall(OpCodes.Call, writeLineObj, null);
            ilg.Emit(OpCodes.Ret);

            // Emit an entry point method; this must be in a
            // non-generic type.
            TypeBuilder dummy = myModule.DefineType("Dummy",
                TypeAttributes.Class | TypeAttributes.NotPublic);
            MethodBuilder entryPoint = dummy.DefineMethod("Main",
                MethodAttributes.Public | MethodAttributes.Static,
                null, null);
            ilg = entryPoint.GetILGenerator();
            // In order to call the static generic method GM, it is
            // necessary to create a constructed type from the 
            // generic type definition for Sample. This can be any
            // constructed type; in this case Sample<int> is used.
            Type SampleOfInt =
                myType.MakeGenericType(typeof(int));
            // Next get a MethodInfo representing the static generic
            // method GM on type Sample<int>.
            MethodInfo SampleOfIntGM = TypeBuilder.GetMethod(SampleOfInt,
                genMethod);
            // Next get a MethodInfo for GM<string>, which is the 
            // instantiation of GM that Main calls.
            MethodInfo GMOfString =
                SampleOfIntGM.MakeGenericMethod(typeof(string));
            // Finally, emit the call. Push a string onto
            // the stack, as the argument for the generic method.
            ilg.Emit(OpCodes.Ldstr, "Hello, world!");
            ilg.EmitCall(OpCodes.Call, GMOfString, null);
            ilg.Emit(OpCodes.Ret);

            myType.CreateType();
            dummy.CreateType();
            myAssembly.SetEntryPoint(entryPoint);
            myAssembly.Save(myAsmName.Name + ".exe");

            Console.WriteLine(myAsmName.Name + ".exe has been saved.");
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

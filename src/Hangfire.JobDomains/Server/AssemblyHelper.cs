using Hangfire.PluginPackets.Interface;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Security.Permissions;

namespace Hangfire.PluginPackets.Server
{
    internal static class _AssemblyHelper
    {
        public static IEnumerable<Type> GetInterfaceTypes<T>(this Assembly assembly)
        {
            var interfaceType = typeof(T);
            var findedTypes = new List<Type>();
            Type[] types = null;
            try
            {
                types = assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                types = e.Types;
            }
            foreach (var type in types.Where(t => t != null))
            {
                if (type.IsClass && !type.IsAbstract
                    && type.GetInterfaces().Any(@interface => @interface.GUID == interfaceType.GUID)
                   )
                {
                    findedTypes.Add(type);
                }
            }
            return findedTypes;
        }

        public static Type GetInterfaceType<T>(Assembly assembly)
        {
            var interfaceType = typeof(T);
            var findedTypes = assembly.GetInterfaceTypes<T>().ToList();
            if (findedTypes.Any())
            {
                Type actualType = findedTypes.First();
                foreach (var type in findedTypes)
                {
                    if (actualType.Assembly.GetName().Version
                        .CompareTo(type.Assembly.GetName().Version) < 0)
                    {
                        actualType = type;
                    }
                }
                return actualType;
            }
            return null;
        }

        public static string ReadReflectionOnlyAssemblyAttribute<A>(this Assembly assembly) where A : Attribute
        {
            var attributes = assembly.GetCustomAttributesData();
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    if (attr.AttributeType.FullName == typeof(A).FullName)
                    {
                        return attr.ConstructorArguments[0].Value.ToString();
                    }
                }
            }
            return string.Empty;
        }

        public static (string Title,string Description) ReadReflectionNameplateAttribute(this Type type) 
        {
            var attributes = type.GetCustomAttributesData();
            if (attributes != null)
            {
                foreach (var attr in attributes)
                {
                    if (attr.AttributeType.FullName == typeof(NameplateAttribute).FullName)
                    {
                        return (attr.ConstructorArguments[0].Value.ToString(), attr.ConstructorArguments[1].Value.ToString());
                    }
                }
            }
            return (string.Empty, string.Empty);
        }

    }
}

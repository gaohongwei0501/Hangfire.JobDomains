using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains
{
    internal static  class ConvertExtension
    {
        public static object ConvertTo(this object convertibleValue, object def)
        {
            if (null == convertibleValue)
            {
                return def;
            }
            var ty = def.GetType();
            if (!ty.IsGenericType)
            {
                return Convert.ChangeType(convertibleValue, ty);
            }
            else
            {
                Type genericTypeDefinition = ty.GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(ty));
                }
            }

            return def;
        }

        public static T ConvertTo<T>(this IConvertible convertibleValue, T def)
        {
            try
            {
                if (null == convertibleValue)
                {
                    return def;
                }

                if (!typeof(T).IsGenericType)
                {
                    return (T)Convert.ChangeType(convertibleValue, typeof(T));
                }
                else
                {
                    Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                    if (genericTypeDefinition == typeof(Nullable<>))
                    {
                        return (T)Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                    }
                }
            }
            catch
            {

            }
            return def;
        }

        public static T ConvertTo<T>(this IConvertible convertibleValue)
        {
            return convertibleValue.ConvertTo<T>(default(T));
        }

        public static DateTime ConvertTo(this string convertibleValue, DateTime def, string format)
        {
            DateTime value = DateTime.MinValue;
            IFormatProvider ifp = new CultureInfo("zh-CN", true);
            if (DateTime.TryParseExact(convertibleValue, format, ifp, DateTimeStyles.None, out value))
            {
                return value;
            }
            return def;
        }

        public static T ConvertTo<T>(this object convertibleValue)
        {

            if (null == convertibleValue) return default(T);

            if (!typeof(T).IsGenericType)
            {
                return (T)Convert.ChangeType(convertibleValue, typeof(T));
            }
            else
            {
                Type genericTypeDefinition = typeof(T).GetGenericTypeDefinition();
                if (genericTypeDefinition == typeof(Nullable<>))
                {
                    return (T)Convert.ChangeType(convertibleValue, Nullable.GetUnderlyingType(typeof(T)));
                }
            }

            return default(T);
        }
    }
}

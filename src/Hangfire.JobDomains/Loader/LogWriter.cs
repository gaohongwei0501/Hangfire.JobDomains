using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Loader
{
    public class LogWriter
    {
        public static TextWriter CreateWriter<T>(string filename)
        {
            var type = typeof(T);
            var provider = File.AppendText($"{AppDomain.CurrentDomain.BaseDirectory}\\log\\{filename}.txt");
            provider.WriteLine("---------------------------------------------------------------------------------------------------");
            provider.WriteLine($" Type: {type.Name},DateTime:{DateTime.Now} ");
            provider.WriteLine("---------------------------------------------------------------------------------------------------");
            return provider;
        }

        public static TextWriter CreateWriter<T>()
        {
            return CreateWriter(typeof(T));
        }

        public static TextWriter CreateWriter(Type type)
        {
            return TextWriter.Null;

#if DEBUG
            var path = $"{AppDomain.CurrentDomain.BaseDirectory}\\log";
            Directory.CreateDirectory(path);
            var provider = File.AppendText($"{path}\\{DateTime.Now.ToString("yyyy-MM-dd")}.txt");
            provider.WriteLine("---------------------------------------------------------------------------------------------------");
            provider.WriteLine($" Type: {type.Name},DateTime:{DateTime.Now} ");
            provider.WriteLine("---------------------------------------------------------------------------------------------------");
            return provider;
#else
            return TextWriter.Null;
#endif

        }
    }
}

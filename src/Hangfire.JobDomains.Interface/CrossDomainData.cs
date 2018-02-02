using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{

    public class CrossDomainData : MarshalByRefObject
    {

        public string PluginDir { get; set; }

        public string assemblyName { get; set; }

        public string typeName { get; set; }

        public object[] paramers { get; set; }

    }

}

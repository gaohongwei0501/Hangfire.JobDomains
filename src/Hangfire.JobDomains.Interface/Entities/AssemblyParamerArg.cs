using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Interface
{
    [Serializable]
    public class AssemblyParamerArg// : MarshalByRefObject
    {
        
        public string PluginDir { get; set; }

        public string AssemblyName { get; set; }

        public string TypeName { get; set; }

        public object[] Paramers { get; set; }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Models
{
    public class ConstructorDefine
    {
        public List<(string Name, string Type)> Paramers = new List<(string Name, string Type)>();
    }
}

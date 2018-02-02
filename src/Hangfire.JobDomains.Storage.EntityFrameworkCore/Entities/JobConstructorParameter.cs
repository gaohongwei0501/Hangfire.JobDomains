using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.Entities
{

    public class JobConstructorParameter : SQLiteEntityBase
    {

        public int PluginId { get; set; }

        public int AssemblyId { get; set; }

        public int JobId { get; set; }

        public string ConstructorGuid { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite.Entities
{

    internal class JobConstructorParameter : SQLiteEntityBase
    {

        public int DomainID { get; set; }

        public int AssemblyID { get; set; }

        public int JobID { get; set; }

        public string ConstructorGuid { get; set; }

        public string Name { get; set; }

        public string Type { get; set; }

    }

}

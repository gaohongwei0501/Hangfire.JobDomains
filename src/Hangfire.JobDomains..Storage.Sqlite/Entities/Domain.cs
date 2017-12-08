using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite.Entities
{
    internal class Domain : SQLiteEntityBase
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public string BasePath { get; set; }
    }
}

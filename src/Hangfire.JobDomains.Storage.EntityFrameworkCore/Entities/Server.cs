using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities
{
    public class Server: SQLiteEntityBase
    {

        public string Name { get; set; }

        public string Description { get; set; }

        public string PlugPath { get; set; }

    }
}

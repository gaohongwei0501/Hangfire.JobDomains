using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite.Entities
{
    internal class Server: SQLiteEntityBase
    {

        public string PlugPath { get; set; }

    }
}

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite.Entities
{
    internal class ServerPlugMap: SQLiteEntityBase
    {

        public ServerPlugMap(string server,string plug)
        {
            ServerName = server;
            PlugName = plug;
        }

        public string ServerName { get; set; }

        public string PlugName { get; set; }

    }
}

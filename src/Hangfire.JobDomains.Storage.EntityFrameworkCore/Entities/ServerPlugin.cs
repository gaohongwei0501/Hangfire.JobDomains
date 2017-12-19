using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities
{
    public class ServerPlugin: SQLiteEntityBase
    {
        public ServerPlugin() { }

        public ServerPlugin(string server,string plug)
        {
            ServerName = server;
            PlugName = plug;
            CreatedAt = DateTime.Now;
        }

        public string ServerName { get; set; }

        public string PlugName { get; set; }

    }
}

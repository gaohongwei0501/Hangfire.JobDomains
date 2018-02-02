using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.Entities
{
    public class ServerQueue : SQLiteEntityBase
    {
        public ServerQueue() { }

        public ServerQueue(string server, string queue)
        {
            ServerName = server;
            QueueName = queue;
            CreatedAt = DateTime.Now;
        }

        public string ServerName { get; set; }

        public string QueueName { get; set; }

    }
}

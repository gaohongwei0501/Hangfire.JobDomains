using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire.PluginPackets.Models;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.Memory
{
    public class MemoryStorage : EFCoreStorage
    {
        public override EFCoreDBContext GetContext() => new MemoryDBContext();

        public override bool TransactionEnable => false;

        public override bool AddService(string nameOrConnectionString)
        {
            return true;
        }
    }
}

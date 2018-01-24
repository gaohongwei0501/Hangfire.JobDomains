using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Memory
{
    public class MemoryDBContext : EFCoreDBContext
    {


        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseInMemoryDatabase("Hangfire.JobDomains.Storage");
            base.OnConfiguring(optionbuilder);
        }

    }

}

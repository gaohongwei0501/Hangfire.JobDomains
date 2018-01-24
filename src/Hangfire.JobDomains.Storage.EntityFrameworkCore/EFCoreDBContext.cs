using Hangfire.JobDomains.Storage.EntityFrameworkCore.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore
{
    public abstract class EFCoreDBContext : DbContext
    {
        public DbSet<Entities.Server> Servers { get; set; }

        public DbSet<Queue> Queues { get; set; }

        public DbSet<ServerPlugin> ServerPlugs { get; set; }

        public DbSet<ServerQueue> ServerQueues { get; set; }

        public DbSet<Plugin> Domains { get; set; }

        public DbSet<Assembly> Assemblies { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<JobConstructorParameter> JobConstructorParameters { get; set; }

    }





}

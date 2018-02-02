using Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer
{
    internal class SqlServerDBContext : EFCoreDBContext
    {
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration<Entities.Server>(new ServerTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.ServerPlugin>(new ServerPluginMapper());
            modelBuilder.ApplyConfiguration<Entities.ServerQueue>(new ServerQueueMapper());
            modelBuilder.ApplyConfiguration<Entities.Queue>(new QueueMapper());
            modelBuilder.ApplyConfiguration<Entities.Plugin>(new PluginMapper());
            modelBuilder.ApplyConfiguration<Entities.Assembly>(new AssemblyTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.Job>(new JobMapper());
            modelBuilder.ApplyConfiguration<Entities.JobConstructorParameter>(new JobConstructorParameterMapper());
        }

        /// <summary>
        /// Add-Migration init
        /// </summary>
        public static string ConnectionString { get; set; } = "DevConnectionString";

        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlServer(ConnectionString);
            base.OnConfiguring(optionbuilder);
        }

        public static bool CanService()
        {
            using (var _context =new SqlServerDBContext())
            {
                _context.Database.Migrate();
            }
            return true;
        }

    }





}

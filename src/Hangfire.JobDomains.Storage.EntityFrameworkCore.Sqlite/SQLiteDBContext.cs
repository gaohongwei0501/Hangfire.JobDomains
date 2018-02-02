using Hangfire.PluginPackets.Storage.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.Sqlite
{
   
    internal class SQLiteDBContext : EFCoreDBContext
    {

        private static bool _created = false;

        public SQLiteDBContext()
        {
            if (!_created)
            {
                _created = true;
                Database.EnsureDeleted();
                Database.EnsureCreated();
            }
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Entities.Server>().ToTable("JobDomains.Server", "Hangfire");
            modelBuilder.Entity<Entities.ServerPlugin>().ToTable("JobDomains.ServerPlugin", "Hangfire");
            modelBuilder.Entity<Entities.ServerQueue>().ToTable("JobDomains.ServerQueue", "Hangfire");
            modelBuilder.Entity<Entities.Queue>().ToTable("JobDomains.Queue", "Hangfire");
            modelBuilder.Entity<Entities.Plugin>().ToTable("JobDomains.Domain", "Hangfire");
            modelBuilder.Entity<Entities.Assembly>().ToTable("JobDomains.Assembly", "Hangfire");
            modelBuilder.Entity<Entities.Job>().ToTable("JobDomains.Job", "Hangfire");
            modelBuilder.Entity<Entities.JobConstructorParameter>().ToTable("JobDomains.JobConstructor", "Hangfire");
        }

        public static string ConnectionString { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionbuilder)
        {
            optionbuilder.UseSqlite(ConnectionString);
            base.OnConfiguring(optionbuilder);
        }

        public static bool CanService()
        {
            var db = new SQLiteDBContext();
            return db.Database.ProviderName != null;
        }


    }





}

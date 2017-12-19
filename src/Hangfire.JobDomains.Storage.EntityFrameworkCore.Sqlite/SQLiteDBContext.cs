using Hangfire.JobDomains.Storage.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.Sqlite
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
            modelBuilder.Entity<Entities.Server>().ToTable("Hangfire.JobDomains.Server");
            modelBuilder.Entity<Entities.ServerPlugin>().ToTable("Hangfire.JobDomains.ServerPlugin");
            modelBuilder.Entity<Entities.Domain>().ToTable("Hangfire.JobDomains.Domain");
            modelBuilder.Entity<Entities.Assembly>().ToTable("Hangfire.JobDomains.Assembly");
            modelBuilder.Entity<Entities.Job>().ToTable("Hangfire.JobDomains.Job");
            modelBuilder.Entity<Entities.JobConstructorParameter>().ToTable("Hangfire.JobDomains.JobConstructor");
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

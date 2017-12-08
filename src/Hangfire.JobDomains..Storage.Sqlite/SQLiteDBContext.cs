using Hangfire.JobDomains.Storage.Sqlite.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.Sqlite
{
   
    internal class SQLiteDBContext : DbContext
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

        public DbSet<ServerGroup> ServerGroups { get; set; }

        public DbSet<Domain> Domains { get; set; }

        public DbSet<Assembly> Assemblies { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<JobConstructor> JobConstructors { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ServerGroup>().ToTable("Hangfire.JobDomains.ServerGroup");
            modelBuilder.Entity<Domain>().ToTable("Hangfire.JobDomains.Domain");
            modelBuilder.Entity<Assembly>().ToTable("Hangfire.JobDomains.Assembly");
            modelBuilder.Entity<Job>().ToTable("Hangfire.JobDomains.Job");
            modelBuilder.Entity<JobConstructor>().ToTable("Hangfire.JobDomains.JobConstructor");
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

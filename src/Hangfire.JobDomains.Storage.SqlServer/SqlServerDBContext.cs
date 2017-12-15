using Hangfire.JobDomains.Storage.SqlServer.Entities;
using Hangfire.JobDomains.Storage.SqlServer.TypeMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.SqlServer
{
    internal class SqlServerDBContext : DbContext
    {

        public SqlServerDBContext()
        {
          
        }

        public DbSet<Entities.Server> Servers { get; set; }

        public DbSet<ServerPlugin> ServerPlugMaps { get; set; }

        public DbSet<Domain> Domains { get; set; }

        public DbSet<Assembly> Assemblies { get; set; }

        public DbSet<Job> Jobs { get; set; }

        public DbSet<JobConstructorParameter> JobConstructorParameters { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.ApplyConfiguration<Entities.Server>(new ServerTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.ServerPlugin>(new ServerPluginMapper());
            modelBuilder.ApplyConfiguration<Entities.Domain>(new DomainMapper());
            modelBuilder.ApplyConfiguration<Entities.Assembly>(new AssemblyTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.Job>(new JobMapper());
            modelBuilder.ApplyConfiguration<Entities.JobConstructorParameter>(new JobConstructorParameterMapper());
        }

        public static string ConnectionString { get; set; }

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

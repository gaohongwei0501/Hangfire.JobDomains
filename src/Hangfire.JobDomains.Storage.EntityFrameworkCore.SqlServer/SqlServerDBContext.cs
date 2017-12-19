using Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.TypeMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer
{
    internal class SqlServerDBContext : EFCoreDBContext
    {
      
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfiguration<Entities.Server>(new ServerTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.ServerPlugin>(new ServerPluginMapper());
            modelBuilder.ApplyConfiguration<Entities.Domain>(new DomainMapper());
            modelBuilder.ApplyConfiguration<Entities.Assembly>(new AssemblyTypeMapper());
            modelBuilder.ApplyConfiguration<Entities.Job>(new JobMapper());
            modelBuilder.ApplyConfiguration<Entities.JobConstructorParameter>(new JobConstructorParameterMapper());
            base.OnModelCreating(modelBuilder);
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

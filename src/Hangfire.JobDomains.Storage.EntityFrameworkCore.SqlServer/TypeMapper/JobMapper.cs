using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    internal class JobMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.Job>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.Job> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.PluginId).IsRequired();
            builder.Property(t => t.AssemblyId).IsRequired();

            builder.Property(t => t.FullName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);

            builder.Property(t => t.Title).HasMaxLength(50);
            builder.Property(t => t.Description).HasMaxLength(200);

            builder.ToTable("Extension_Job", "Hangfire");
        }
    }
}

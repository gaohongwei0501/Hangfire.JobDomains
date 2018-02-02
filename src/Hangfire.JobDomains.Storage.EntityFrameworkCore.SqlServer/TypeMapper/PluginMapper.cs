using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    internal class PluginMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.Plugin>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.Plugin> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.Title).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Description).HasMaxLength(200);

            builder.ToTable("Extension_Plugin", "Hangfire");
        }
    }
}

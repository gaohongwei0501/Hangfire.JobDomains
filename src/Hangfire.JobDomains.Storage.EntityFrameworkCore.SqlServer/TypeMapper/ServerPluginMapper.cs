using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{

    internal class ServerPluginMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.ServerPlugin>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.ServerPlugin> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.ServerName).IsRequired().HasMaxLength(50);
            builder.Property(t => t.PlugName).IsRequired().HasMaxLength(50);

            builder.ToTable("Extension_ServerPlugin", "Hangfire");
        }
    }

}

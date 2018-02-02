using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{


    internal class ServerTypeMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.Server>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.Server> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.PlugPath).IsRequired().HasMaxLength(500);
            builder.Property(t => t.Description).HasMaxLength(200);

            builder.ToTable("Extension_Server", "Hangfire");
        }
    }
}

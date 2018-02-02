using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{

    internal class ServerQueueMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.ServerQueue>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.ServerQueue> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.ServerName).IsRequired().HasMaxLength(50);
            builder.Property(t => t.QueueName).IsRequired().HasMaxLength(50);

            builder.ToTable("Extension_ServerQueue", "Hangfire");
        }
    }
}

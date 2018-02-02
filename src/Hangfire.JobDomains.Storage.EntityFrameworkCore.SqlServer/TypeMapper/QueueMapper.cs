using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    public class QueueMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.Queue>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.Queue> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Name);

            // Properties
            builder.Property(t => t.Description).IsRequired().HasMaxLength(50);
            builder.Property(t => t.CreatedAt).IsRequired();

            builder.ToTable("Extension_Queue", "Hangfire");
        }
    }
}

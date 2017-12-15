using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.SqlServer.TypeMapper
{


    internal class ServerTypeMapper : IEntityTypeConfiguration<Entities.Server>
    {
        public void Configure(EntityTypeBuilder<Entities.Server> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);

            // Properties
            builder.Property(t => t.ID).UseSqlServerIdentityColumn();

            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.PlugPath).IsRequired().HasMaxLength(500);
            builder.Property(t => t.Description).HasMaxLength(200);

            builder.ToTable("Hangfire.JobDomains.Server");
        }
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{

    internal class ServerQueueMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.ServerQueue>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.ServerQueue> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);

            // Properties
            builder.Property(t => t.ID).UseSqlServerIdentityColumn();

            builder.Property(t => t.ServerName).IsRequired().HasMaxLength(50);
            builder.Property(t => t.QueueName).IsRequired().HasMaxLength(50);

            builder.ToTable("JobDomains.ServerQueue", "Hangfire");
        }
    }
}

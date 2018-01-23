using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{

    internal class ServerPluginMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.ServerPlugin>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.ServerPlugin> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);

            // Properties
            builder.Property(t => t.ID).UseSqlServerIdentityColumn();

            builder.Property(t => t.ServerName).IsRequired().HasMaxLength(50);
            builder.Property(t => t.PlugName).IsRequired().HasMaxLength(50);

            builder.ToTable("JobDomains.ServerPlugin", "Hangfire");
        }
    }

}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{

    internal class AssemblyTypeMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.Assembly>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.Assembly> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);

            // Properties
            builder.Property(t => t.ID).UseSqlServerIdentityColumn();

            builder.Property(t => t.PluginID).IsRequired();

            builder.Property(t => t.FileName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.FullName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.ShortName).IsRequired().HasMaxLength(50);

            builder.Property(t => t.Title).HasMaxLength(50);
            builder.Property(t => t.Description).HasMaxLength(200);

            builder.ToTable("JobDomains.Assembly", "Hangfire");
        }
    }

}

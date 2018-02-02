using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    internal class JobConstructorParameterMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.JobConstructorParameter>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.JobConstructorParameter> builder)
        {
            // Primary Key
            builder.HasKey(t => t.Id);

            // Properties
            builder.Property(t => t.Id).UseSqlServerIdentityColumn();

            builder.Property(t => t.PluginId ).IsRequired();
            builder.Property(t => t.AssemblyId).IsRequired();
            builder.Property(t => t.JobId).IsRequired();

            builder.Property(t => t.ConstructorGuid).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Type).IsRequired().HasMaxLength(50);

            builder.ToTable("Extension_JobConstructorParameter", "Hangfire");
        }
    }
}

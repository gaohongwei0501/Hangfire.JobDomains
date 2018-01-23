using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
namespace Hangfire.JobDomains.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    internal class JobConstructorParameterMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.JobConstructorParameter>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.JobConstructorParameter> builder)
        {
            // Primary Key
            builder.HasKey(t => t.ID);

            // Properties
            builder.Property(t => t.ID).UseSqlServerIdentityColumn();

            builder.Property(t => t.DomainID).IsRequired();
            builder.Property(t => t.AssemblyID).IsRequired();
            builder.Property(t => t.JobID).IsRequired();

            builder.Property(t => t.ConstructorGuid).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Name).IsRequired().HasMaxLength(50);
            builder.Property(t => t.Type).IsRequired().HasMaxLength(50);

            builder.ToTable("JobDomains.JobConstructorParameter", "Hangfire");
        }
    }
}

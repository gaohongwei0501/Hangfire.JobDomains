using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangfire.PluginPackets.Storage.EntityFrameworkCore.SqlServer.TypeMapper
{
    public class QueuePlanMapper : IEntityTypeConfiguration<EntityFrameworkCore.Entities.QueuePlan>
    {
        public void Configure(EntityTypeBuilder<EntityFrameworkCore.Entities.QueuePlan> builder)
        {
            // Primary Key
            builder.HasKey(t => t.PlanName);

            // Properties
            builder.Property(t => t.QueueName).IsRequired().HasMaxLength(100);
            builder.Property(t => t.CreatedAt).IsRequired();


            builder.Property(t => t.Period).IsRequired().HasMaxLength(200);
            builder.Property(t => t.PlugName).IsRequired().HasMaxLength(200);
            builder.Property(t => t.AssemblyName).IsRequired();
            builder.Property(t => t.TypeName).IsRequired();
            builder.Property(t => t.Args).IsRequired().HasMaxLength(500);

            builder.ToTable("Extension_QueuePlan", "Hangfire");
        }
    }
}

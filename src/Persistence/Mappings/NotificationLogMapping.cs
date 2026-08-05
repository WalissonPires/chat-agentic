using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class NotificationLogMapping : IEntityTypeConfiguration<NotificationLog>
    {
        public void Configure(EntityTypeBuilder<NotificationLog> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ExecutionPeriodKey).HasMaxLength(10).IsRequired();

            builder.HasOne(x => x.NotificationRule).WithMany().HasForeignKey(x => x.NotificationRuleId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne(x => x.Person).WithMany().HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.NotificationRuleId, x.PersonId, x.ExecutionPeriodKey })
                .HasFilter($"status = '{nameof(NotificationLogStatus.Sent)}'")
                .IsUnique();

            builder.HasIndex(x => new { x.NotificationRuleId, x.ExecutionBatchId });
            builder.HasIndex(x => new { x.NotificationRuleId, x.SentAt });
        }
    }
}

using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class NotificationRuleMapping : IEntityTypeConfiguration<NotificationRule>
    {
        public void Configure(EntityTypeBuilder<NotificationRule> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(120).IsRequired();
            builder.Property(x => x.MessageTemplate).IsRequired();
            builder.Property(x => x.SendTime).IsRequired();

            builder.OwnsMany(x => x.TargetFilters, tf => tf.ToJson());

            builder.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.Enabled, x.NextExecutionAt });
            builder.HasIndex(x => x.WorkspaceId);
        }
    }
}

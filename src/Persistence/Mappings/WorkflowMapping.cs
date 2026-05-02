using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class WorkflowMapping : IEntityTypeConfiguration<Workflow>
    {
        public void Configure(EntityTypeBuilder<Workflow> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(60).IsRequired();
            builder.Property(x => x.WebhookToken).HasMaxLength(32).IsRequired();

            builder.OwnsOne(x => x.Metadata, meta =>
            {
                meta.ToJson();
                meta.OwnsOne(m => m.Agent);
                meta.OwnsOne(m => m.EvolutionApi);
                meta.OwnsOne(m => m.Telegram);
            });

            builder.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.WebhookToken).IsUnique();
            builder.HasIndex(x => x.WorkspaceId);
        }
    }
}

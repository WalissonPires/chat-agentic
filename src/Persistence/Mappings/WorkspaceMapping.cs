using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class WorkspaceMapping : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(60).IsRequired(true);
            builder.Property(x => x.IntegrationToken).HasMaxLength(32);
            builder.OwnsOne(x => x.Metadata, meta =>
            {
                meta.ToJson();
                meta.OwnsOne(m => m.AIProvider);
            });

            builder.HasMany(x => x.Channels)
                .WithOne(c => c.Workspace)
                .HasForeignKey(c => c.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.IntegrationToken);
        }
    }
}
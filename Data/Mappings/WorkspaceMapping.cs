using ChatAgentic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Data.Mappings
{
    public class WorkspaceMapping : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(60).IsRequired(true);
            builder.Property(x => x.WebhookToken).HasMaxLength(32);
            builder.OwnsOne(x => x.Metadata, x => x.ToJson());

            builder.HasIndex(x => x.WebhookToken);
        }
    }
}
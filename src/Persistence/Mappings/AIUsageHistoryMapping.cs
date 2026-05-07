using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings;

public class AIUsageHistoryMapping : IEntityTypeConfiguration<AIUsageHistory>
{
    public void Configure(EntityTypeBuilder<AIUsageHistory> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Provider).HasMaxLength(64).IsRequired();
        builder.Property(x => x.Cost).HasColumnType("numeric(18,6)").IsRequired();

        builder.HasIndex(x => new { x.WorkspaceId, x.CreatedAt });
        builder.HasIndex(x => x.ConversationId);
        builder.HasIndex(x => new { x.Provider, x.Service });
    }
}

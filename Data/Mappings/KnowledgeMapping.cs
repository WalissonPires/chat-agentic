using ChatAgentic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Data.Mappings
{
    public class KnowledgeMapping : IEntityTypeConfiguration<Knowledge>
    {
        public void Configure(EntityTypeBuilder<Knowledge> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Context).HasMaxLength(120).IsRequired();
            builder.Property(x => x.Content).HasMaxLength(5000);
            builder.Property(x => x.Embedding).HasColumnType("VECTOR(1536)").IsRequired();

            builder.HasOne(x => x.Workspace).WithMany().HasForeignKey(x => x.WorkspaceId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.WorkspaceId, x.Context });

            builder.HasIndex(x => x.Embedding)
                .HasMethod("HNSW")
                .HasOperators("vector_cosine_ops")
                .HasStorageParameter("m", 16)
                .HasStorageParameter("ef_construction", 64);
        }
    }
}
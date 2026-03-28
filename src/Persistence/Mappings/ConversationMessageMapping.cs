using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class ConversationMessageMapping : IEntityTypeConfiguration<ConversationMessage>
    {
        public void Configure(EntityTypeBuilder<ConversationMessage> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.MessageId).HasMaxLength(50);
            builder.Property(x => x.Role).HasMaxLength(30);
            builder.Property(x => x.FileName).HasMaxLength(120);
            builder.Property(x => x.ContentText).HasMaxLength(5000);
            builder.Property(x => x.MediaUri).HasMaxLength(2000);
            builder.Property(x => x.MimeType).HasMaxLength(30);

            builder.HasOne(x => x.Conversation).WithMany(x => x.Messages).HasForeignKey(x => x.ConversationId).OnDelete(DeleteBehavior.Cascade);
        }
    }
}
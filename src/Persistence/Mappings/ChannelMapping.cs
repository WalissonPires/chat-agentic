using ChatAgentic.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Persistence.Mappings
{
    public class ChannelMapping : IEntityTypeConfiguration<Channel>
    {
        public void Configure(EntityTypeBuilder<Channel> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name).HasMaxLength(60).IsRequired();

            builder.OwnsOne(x => x.Credentials, cred =>
            {
                cred.ToJson();
                cred.OwnsOne(c => c.EvolutionApi);
                cred.OwnsOne(c => c.Telegram);
            });

            builder.HasOne(x => x.Workspace)
                .WithMany(w => w.Channels)
                .HasForeignKey(x => x.WorkspaceId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => x.WorkspaceId);
        }
    }
}

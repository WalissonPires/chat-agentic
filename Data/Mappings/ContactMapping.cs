using ChatAgentic.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ChatAgentic.Data.Mappings
{
    public class ContactMapping : IEntityTypeConfiguration<Contact>
    {
        public void Configure(EntityTypeBuilder<Contact> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Identifier).HasMaxLength(32).IsRequired();

            builder.HasOne(x => x.Person).WithMany(x => x.Contacts).HasForeignKey(x => x.PersonId).OnDelete(DeleteBehavior.Cascade);

            builder.HasIndex(x => new { x.Channel, x.Identifier });
        }
    }
}
using ChatAgentic.Features.Channels;

namespace ChatAgentic.Entities
{
    public class Contact
    {
        public int Id { get; set; }
        public ChannelType Channel { get; set; }
        public string Identifier { get; set; } = string.Empty;

        public int PersonId { get; set; }
        public Person Person { get; set; } = default!;
    }
}
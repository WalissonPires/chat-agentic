using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Entities
{
    public class Channel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public ChannelType Type { get; set; }
        public ChannelCredentials? Credentials { get; set; }

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;
    }

    public class ChannelCredentials
    {
        public EvolutionApiOptions? EvolutionApi { get; set; }
        public TelegramApiOptions? Telegram { get; set; }
    }
}

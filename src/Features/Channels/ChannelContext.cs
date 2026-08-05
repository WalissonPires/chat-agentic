using ChatAgentic.Entities;
using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Features.Channels
{
    public sealed class ChannelContext
    {
        private EvolutionApiOptions? _evolutionApiOptions;
        private TelegramApiOptions? _telegramApiOptions;

        public void SetFromChannel(Channel channel)
        {
            if (channel.Type == ChannelType.Whatsapp)
            {
                _evolutionApiOptions = channel.Credentials?.EvolutionApi;
            }
            else if (channel.Type == ChannelType.Telegram)
            {
                _telegramApiOptions = channel.Credentials?.Telegram;
            }
        }

        public void SetFromAgentDefinition(AgentDefinition agentDefinition)
        {
            if (agentDefinition.WhatsappChannel != null)
            {
                SetFromChannel(agentDefinition.WhatsappChannel);
            }

            if (agentDefinition.TelegramChannel != null)
            {
                SetFromChannel(agentDefinition.TelegramChannel);
            }
        }

        public EvolutionApiOptions GetEvolutionApiOptions()
            => _evolutionApiOptions ?? throw new InvalidOperationException("WhatsApp channel is not configured.");

        public TelegramApiOptions GetTelegramApiOptions()
            => _telegramApiOptions ?? throw new InvalidOperationException("Telegram channel is not configured.");
    }
}

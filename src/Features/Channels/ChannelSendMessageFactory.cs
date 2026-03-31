using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Features.Channels
{
    public class ChannelSendMessageFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChannelSendMessageFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }


        public IChannelSendMessage Create(ChannelType channel)
        {
            return channel switch
            {
                ChannelType.Whatsapp => _serviceProvider.GetRequiredService<WhatsappSendMessage>(),
                ChannelType.Telegram => _serviceProvider.GetRequiredService<TelegramSendMessage>(),
                _ => throw new NotSupportedException($"Channel not support send message")
            };
        }
    }
}
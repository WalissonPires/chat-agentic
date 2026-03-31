using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;

namespace ChatAgentic.Features.Channels
{
    public class ChannelMessageTransformFactory
    {
        private readonly IServiceProvider _serviceProvider;

        public ChannelMessageTransformFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IChannelMessageTransform Create(ChannelType channel)
        {
            return channel switch
            {
                ChannelType.Whatsapp => _serviceProvider.GetRequiredService<WhatsappMessageTransform>(),
                ChannelType.Telegram => _serviceProvider.GetRequiredService<TelegramMessageTransform>(),
                _ => throw new Exception("Invalid channel " + channel)
            };
        }
    }
}
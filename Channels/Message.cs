namespace ChatAgentic.Channels
{
    public record Message(
        ChannelType Channel,
        string SenderIdentifier,
        MessageContentType ContentType,
        string ContentText,
        string? MediaUri = null,
        string? FileName = null,
        string? MimeType = null
    );

    public enum MessageContentType
    {
        Text = 1,
        Audio = 2,
        Image = 3,
        Video = 4,
        Document = 5
    }
}
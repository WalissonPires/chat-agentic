using ChatAgentic.Utils;
using Microsoft.Extensions.AI;

namespace ChatAgentic.src.Features.AI
{
    public static class ChatMessageExtensions
    {
        public static async Task LoadFileToBase64(this UriContent content)
        {
            if (content.Uri.Scheme == "file")
            {
                var fileBytes = await File.ReadAllBytesAsync(content.Uri.LocalPath);
                var dataUri = new DataUri(content.MediaType, Convert.ToBase64String(fileBytes));
                content.Uri = new Uri(dataUri.ToString());
            }
        }
    }
}
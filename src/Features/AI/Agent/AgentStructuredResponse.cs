using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.AI.Agent
{
    public sealed class AgentTextSegment
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("label")]
        public string Label { get; set; } = string.Empty;

        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public sealed partial class AgentStructuredResponse
    {
        private const string UrlSegmentType = "url";
        private const string DefaultUrlSegmentLabel = "Link";

        [JsonPropertyName("speakableText")]
        public string SpeakableText { get; set; } = string.Empty;

        [JsonPropertyName("textSegments")]
        public List<AgentTextSegment> TextSegments { get; set; } = [];

        public AgentStructuredResponse Normalize()
        {
            SpeakableText = (SpeakableText ?? string.Empty).Trim();
            var normalizedSegments = NormalizeTextSegments(TextSegments);
            var orderedUrls = CollectUrlsInOrder([SpeakableText, ..normalizedSegments.Select(static segment => segment.Value)]);

            var proseParts = new List<string>();
            var speakableWithoutUrls = StripUrlsToProse(SpeakableText);
            if (!string.IsNullOrWhiteSpace(speakableWithoutUrls))
                proseParts.Add(speakableWithoutUrls);

            foreach (var segment in normalizedSegments)
            {
                var prose = StripUrlsToProse(segment.Value);
                if (!string.IsNullOrWhiteSpace(prose))
                    proseParts.Add(prose);
            }

            SpeakableText = CollapseWhitespaceRegex().Replace(string.Join(" ", proseParts), " ").Trim();
            TextSegments = orderedUrls
                .Select(static url => new AgentTextSegment
                {
                    Type = UrlSegmentType,
                    Label = DefaultUrlSegmentLabel,
                    Value = url
                })
                .ToList();
            return this;
        }

        private static string StripUrlsToProse(string source)
        {
            if (string.IsNullOrWhiteSpace(source))
                return string.Empty;

            var prose = UrlRegex().Replace(source, " ").Trim();
            return CollapseWhitespaceRegex().Replace(prose, " ").Trim();
        }

        private static List<string> CollectUrlsInOrder(IEnumerable<string> sources)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var list = new List<string>();

            foreach (var source in sources)
            {
                if (string.IsNullOrWhiteSpace(source))
                    continue;

                foreach (Match m in UrlRegex().Matches(source))
                {
                    var url = TrimTrailingPunctuationFromUrl(m.Value);
                    if (string.IsNullOrWhiteSpace(url) || !seen.Add(url))
                        continue;
                    list.Add(url);
                }
            }

            return list;
        }

        public static AgentStructuredResponse FromPlainText(string text)
        {
            var normalizedText = (text ?? string.Empty).Trim();
            var urls = CollectUrlsInOrder([normalizedText]);
            var speakableText = StripUrlsToProse(normalizedText);

            return new AgentStructuredResponse
            {
                SpeakableText = speakableText,
                TextSegments = urls
                    .Select(static url => new AgentTextSegment
                    {
                        Type = UrlSegmentType,
                        Label = DefaultUrlSegmentLabel,
                        Value = url
                    })
                    .ToList()
            };
        }

        public ChatMessage ToChatMessage()
        {
            var chatMessage = new ChatMessage
            {
                MessageId = Guid.NewGuid().ToString("N"),
                Role = ChatRole.Assistant,
                CreatedAt = DateTime.UtcNow,
                Contents = []
            };

            if (!string.IsNullOrWhiteSpace(SpeakableText))
                chatMessage.Contents.Add(new TextContent(SpeakableText));

            foreach (var segment in TextSegments)
            {
                if (!string.IsNullOrWhiteSpace(segment.Value))
                    chatMessage.Contents.Add(new TextContent(segment.Value));
            }

            return chatMessage;
        }

        /// <summary>
        /// Mensagens separadas para envio ao canal: opcionalmente omite o texto falável se ele já foi enviado como áudio.
        /// </summary>
        public IEnumerable<ChatMessage> ToOutboundChatMessages(bool omitSpeakableText)
        {
            var createdAt = DateTime.UtcNow;

            if (!omitSpeakableText && !string.IsNullOrWhiteSpace(SpeakableText))
            {
                yield return new ChatMessage
                {
                    MessageId = Guid.NewGuid().ToString("N"),
                    Role = ChatRole.Assistant,
                    CreatedAt = createdAt,
                    Contents = [new TextContent(SpeakableText)]
                };
            }

            foreach (var segment in TextSegments)
            {
                if (string.IsNullOrWhiteSpace(segment.Value))
                    continue;

                yield return new ChatMessage
                {
                    MessageId = Guid.NewGuid().ToString("N"),
                    Role = ChatRole.Assistant,
                    CreatedAt = createdAt,
                    Contents = [new TextContent(string.IsNullOrEmpty(segment.Label) ? segment.Value : $"{segment.Label}: {segment.Value}")]
                };
            }
        }

        private static List<AgentTextSegment> NormalizeTextSegments(IEnumerable<AgentTextSegment>? segments)
        {
            if (segments is null)
                return [];

            var normalized = new List<AgentTextSegment>();
            foreach (var segment in segments)
            {
                if (segment is null || string.IsNullOrWhiteSpace(segment.Value))
                    continue;

                var value = segment.Value.Trim();
                if (!value.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !value.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                normalized.Add(new AgentTextSegment
                {
                    Type = UrlSegmentType,
                    Label = string.IsNullOrWhiteSpace(segment.Label) ? DefaultUrlSegmentLabel : segment.Label.Trim(),
                    Value = value
                });
            }

            return normalized;
        }

        private static string TrimTrailingPunctuationFromUrl(string url)
        {
            while (url.Length > 0 && "()[]\"'.,;:!".Contains(url[^1]))
                url = url[..^1];

            return url.Trim();
        }

        [GeneratedRegex(@"https?://\S+", RegexOptions.Compiled | RegexOptions.IgnoreCase)]
        private static partial Regex UrlRegex();

        [GeneratedRegex(@"\s+", RegexOptions.Compiled)]
        private static partial Regex CollapseWhitespaceRegex();
    }
}

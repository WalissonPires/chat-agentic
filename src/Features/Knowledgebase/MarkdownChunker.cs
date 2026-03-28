using System.Text;

namespace ChatAgentic.Features.Knowledgebase
{
    public class MarkdownChunker
    {
        public static IEnumerable<string> Split(string content, int maxChars = 1000)
        {
            if (string.IsNullOrWhiteSpace(content))
                yield break;

            var lines = content.Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
            var currentHeaders = new Dictionary<int, string>();
            var currentBuffer = new StringBuilder();
            var currentPath = "";

            foreach (var line in lines)
            {
                var trimmed = line.Trim();

                if (trimmed.StartsWith('#'))
                {
                    int headerLevel = 0;
                    while (headerLevel < trimmed.Length && trimmed[headerLevel] == '#')
                    {
                        headerLevel++;
                    }

                    if (headerLevel > 0 && headerLevel < trimmed.Length && trimmed[headerLevel] == ' ')
                    {
                        if (currentBuffer.Length > 0)
                        {
                            foreach (var chunk in CreateChunks(currentPath, currentBuffer.ToString().Trim(), maxChars))
                            {
                                yield return chunk;
                            }
                            currentBuffer.Clear();
                        }

                        var headerText = trimmed[headerLevel..].Trim();

                        var keysToRemove = new List<int>();
                        foreach (var key in currentHeaders.Keys)
                        {
                            if (key >= headerLevel)
                                keysToRemove.Add(key);
                        }
                        foreach (var key in keysToRemove)
                            currentHeaders.Remove(key);

                        currentHeaders[headerLevel] = headerText;

                        var pathParts = new List<string>();
                        for (int i = 1; i <= 6; i++)
                        {
                            if (currentHeaders.TryGetValue(i, out var val))
                                pathParts.Add(val);
                        }
                        currentPath = string.Join(" > ", pathParts);
                        continue;
                    }
                }

                if (!string.IsNullOrWhiteSpace(trimmed))
                {
                    currentBuffer.AppendLine(trimmed);
                }
            }

            if (currentBuffer.Length > 0)
            {
                foreach (var chunk in CreateChunks(currentPath, currentBuffer.ToString().Trim(), maxChars))
                {
                    yield return chunk;
                }
            }
        }

        private static IEnumerable<string> CreateChunks(string prefix, string content, int maxChars)
        {
            if (string.IsNullOrWhiteSpace(content))
                yield break;

            string prefixStr = string.IsNullOrEmpty(prefix) ? "" : $"[{prefix}]\n";
            int contentMax = maxChars - prefixStr.Length;

            if (contentMax <= 0 || content.Length <= contentMax)
            {
                yield return prefixStr + content;
            }
            else
            {
                var chunks = CharacterChunker.Split(content, contentMax);
                foreach (var chunk in chunks)
                {
                    yield return prefixStr + chunk;
                }
            }
        }
    }
}
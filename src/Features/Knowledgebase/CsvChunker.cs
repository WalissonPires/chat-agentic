namespace ChatAgentic.Features.Knowledgebase
{
    public static class CsvChunker
    {
        public static IEnumerable<string> Split(string csvContent, char delimiter = ',')
        {
            if (string.IsNullOrWhiteSpace(csvContent))
                yield break;

            var lines = csvContent
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(l => l.Trim())
                .Where(l => !string.IsNullOrWhiteSpace(l))
                .ToArray();

            if (lines.Length < 2)
                yield break;

            var headers = lines[0].Split(delimiter).Select(h => h.Trim()).ToArray();

            for (int i = 1; i < lines.Length; i++)
            {
                var values = lines[i].Split(delimiter).Select(v => v.Trim()).ToArray();

                var parts = headers
                    .Zip(values, (header, value) => $"{header}: {value}")
                    .ToArray();

                yield return string.Join(", ", parts);
            }
        }
    }
}
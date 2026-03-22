namespace ChatAgentic.Services
{
    public static class DocumentChunker
    {
        public static IEnumerable<string> Split(string content, ChunkerType chunkerType)
        {
            return chunkerType switch
            {
                ChunkerType.CSV => CsvChunker.Split(content),
                ChunkerType.Markdown => MarkdownChunker.Split(content),
                ChunkerType.LineBreak => LineBreakChunker.Split(content),
                ChunkerType.Character => CharacterChunker.Split(content),
                _ => CharacterChunker.Split(content)
            };
        }
    }

    public enum ChunkerType
    {
        LineBreak = 1,
        CSV = 2,
        Markdown = 3,
        Character = 4
    }
}
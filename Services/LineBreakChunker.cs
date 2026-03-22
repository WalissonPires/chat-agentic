namespace ChatAgentic.Services
{
    public class LineBreakChunker
    {
        public static IEnumerable<string> Split(string text, string lineBreak = "\r\n")
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            var lines = text.Split(lineBreak);

            foreach (var line in lines)
            {
                yield return line;
            }
        }
    }
}
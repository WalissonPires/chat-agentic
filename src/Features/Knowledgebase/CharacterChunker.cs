namespace ChatAgentic.Features.Knowledgebase
{
    public class CharacterChunker
    {
        public static IEnumerable<string> Split(string text, int maxChars = 800)
        {
            if (string.IsNullOrWhiteSpace(text))
                yield break;

            var words = text.Split(' ');
            var buffer = new List<string>();
            int currentLength = 0;

            foreach (var word in words)
            {
                if (currentLength + word.Length + 1 > maxChars)
                {
                    yield return string.Join(" ", buffer);
                    buffer.Clear();
                    currentLength = 0;
                }

                buffer.Add(word);
                currentLength += word.Length + 1;
            }

            if (buffer.Count > 0)
                yield return string.Join(" ", buffer);
        }
    }
}
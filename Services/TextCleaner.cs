namespace ChatAgentic.Services
{
    public class TextCleaner
    {
        public static string Clean(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;

            var cleaned = text
                //.Replace("\r", " ") // break CSV
                //.Replace("\n", " ") // break CSV
                .Replace("\t", " ")
                .Replace("  ", " ");

            while (cleaned.Contains("  "))
            {
                cleaned = cleaned.Replace("  ", " ");
            }

            return cleaned.Trim();
        }
    }
}
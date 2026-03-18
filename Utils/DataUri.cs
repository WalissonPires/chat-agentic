namespace ChatAgentic.Utils
{
    public class DataUri
    {
        public string MimeType { get; private set; } = "application/octet-stream";
        public string Base64 { get; private set; } = string.Empty;

        public DataUri(string dataUri)
        {
            Parse(dataUri);
        }

        private void Parse(string dataUri)
        {
            int commaIdx = dataUri.IndexOf(',');
            if (commaIdx < 0)
                throw new ArgumentException("Invalid data-URI: missing comma separator.", nameof(dataUri));

            // Extract MIME from "data:<mime>;base64"
            var meta = dataUri[5..commaIdx];              // e.g. "audio/webm;base64"
            MimeType = meta.Split(';')[0];                // e.g. "audio/webm"
            Base64 = dataUri[(commaIdx + 1)..];           // raw Base64 bytes
        }
    }
}
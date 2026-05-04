namespace ChatAgentic.Features.AI.Audio
{
    public static class AudioTranscriptionFormats
    {
        public static string MapMimeToFormat(string mimeType)
        {
            var m = mimeType.ToLowerInvariant();
            if (m.StartsWith("audio/webm", StringComparison.Ordinal))
                return "webm";
            if (m.StartsWith("audio/ogg", StringComparison.Ordinal))
                return "ogg";
            if (m is "audio/mp4" or "audio/m4a" || m.StartsWith("audio/mp4;", StringComparison.Ordinal))
                return "m4a";
            if (m is "audio/mpeg" or "audio/mp3" || m.StartsWith("audio/mpeg;", StringComparison.Ordinal))
                return "mp3";
            if (m is "audio/wav" or "audio/wave" || m.StartsWith("audio/wav;", StringComparison.Ordinal))
                return "wav";
            if (m.StartsWith("audio/flac", StringComparison.Ordinal))
                return "flac";
            if (m.StartsWith("audio/aac", StringComparison.Ordinal))
                return "aac";
            return "webm";
        }
    }
}

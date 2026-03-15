namespace ChatAgentic.Channels
{
    public class EvolutionWebhookPayload
    {
        public string? Event { get; set; }
        public string? Instance { get; set; }
        public EvolutionPayloadData? Data { get; set; }
        public string? Destination { get; set; }
        public string? DateTime { get; set; }
        public string? Sender { get; set; }
        public string? ServerUrl { get; set; }
        public string? Apikey { get; set; }
    }

    public class EvolutionPayloadData
    {
        public EvolutionPayloadKey? Key { get; set; }
        public string? PushName { get; set; }
        public string? Status { get; set; }
        public EvolutionPayloadInfo? Message { get; set; }
        public string? MessageType { get; set; }
        public long MessageTimestamp { get; set; }
        public string? InstanceId { get; set; }
        public string? Source { get; set; }
    }

    public class EvolutionPayloadKey
    {
        public string? RemoteJid { get; set; }
        public string? RemoteJidAlt { get; set; }
        public bool FromMe { get; set; }
        public string? Id { get; set; }
        public string? Participant { get; set; }
        public string? AddressingMode { get; set; }
    }

    public class EvolutionPayloadInfo
    {
        public string? Conversation { get; set; }

        public EvolutionAudioMessage? AudioMessage { get; set; }

        public EvolutionImageMessage? ImageMessage { get; set; }

        public EvolutionDocumentMessage? DocumentMessage { get; set; }

        public EvolutionMessageContextInfo? MessageContextInfo { get; set; }
    }

    public class EvolutionAudioMessage
    {

        public string? Url { get; set; }


        public string? Mimetype { get; set; }


        public int Seconds { get; set; }


        public bool Ptt { get; set; }
    }

    public class EvolutionImageMessage
    {

        public string? Url { get; set; }


        public string? Mimetype { get; set; }


        public string? Caption { get; set; }


        public int Height { get; set; }


        public int Width { get; set; }
    }

    public class EvolutionDocumentMessage
    {

        public string? Url { get; set; }


        public string? Mimetype { get; set; }


        public string? FileName { get; set; }


        public string? Caption { get; set; }


        public EvolutionFileLength? FileLength { get; set; }
    }

    public class EvolutionMessageContextInfo
    {

        public List<object> ThreadId { get; set; } = [];


        public EvolutionDeviceListMetadata? DeviceListMetadata { get; set; }


        public int DeviceListMetadataVersion { get; set; }

        /// <summary>
        /// Dictionary where keys are string indices ("0", "1", ...) and values are byte integers.
        /// </summary>

        public Dictionary<string, int>? MessageSecret { get; set; }
    }

    public class EvolutionDeviceListMetadata
    {

        public List<object> SenderKeyIndexes { get; set; } = [];


        public List<object> RecipientKeyIndexes { get; set; } = [];

        /// <summary>Keys are string indices ("0"…"9"), values are byte integers.</summary>

        public Dictionary<string, int>? SenderKeyHash { get; set; }


        public EvolutionTimestamp? SenderTimestamp { get; set; }


        public Dictionary<string, int>? RecipientKeyHash { get; set; }


        public EvolutionTimestamp? RecipientTimestamp { get; set; }
    }

    public class EvolutionTimestamp
    {

        public long Low { get; set; }


        public long High { get; set; }


        public bool Unsigned { get; set; }
    }

    public class EvolutionFileLength
    {


        public int Low { get; set; }



        public int High { get; set; }



        public bool Unsigned { get; set; }
    }

    public class EvolutionMediaBase64
    {


        public string? MediaType { get; set; }



        public string? FileName { get; set; }



        public EvolutionMediaSize? Size { get; set; }



        public string? Mimetype { get; set; }



        public string? Base64 { get; set; }



        public object? Buffer { get; set; }
    }

    public class EvolutionMediaSize
    {


        public EvolutionFileLength? FileLength { get; set; }
    }
}

namespace ChatAgentic.Features.Knowledgebase
{
    public class KnowledgeIngestionDTO
    {
        public string? Context { get; set; }
        public ChunkerType ChunkerType { get; set; }
        public bool? ClearText { get; set; }
        public string? Token { get; set; }
        public IFormFile? File { get; set; }
    }
}
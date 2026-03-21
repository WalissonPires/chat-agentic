using Pgvector;

namespace ChatAgentic.Models
{
    public class Knowledge
    {
        public int Id { get; set; }
        public string Context { get; set;} = default!;
        public string Source { get; set; } = default!;
        public string Content { get; set; } = default!;
        public Vector Embedding { get; set; } = default!;

        public int WorkspaceId { get; set; }
        public Workspace Workspace { get; set; } = default!;
    }
}
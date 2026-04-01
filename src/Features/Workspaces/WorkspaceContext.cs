using ChatAgentic.Entities;

namespace ChatAgentic.Features.Workspaces;

public sealed class WorkspaceContext
{
    private WorkspaceMetadata? _metadata;

    public WorkspaceMetadata Metadata => _metadata ?? throw new InvalidOperationException("WorkspaceContext is not initialized. Call SetMetadata before resolving tenant-scoped services.");

    public void SetMetadata(Workspace workspace)
    {
        _metadata = workspace.Metadata;
    }
}

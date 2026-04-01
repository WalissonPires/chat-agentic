using ChatAgentic.Entities;
using ChatAgentic.Features.Workspaces;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence;

public sealed class WorkspaceLoader
{
    private readonly AppDbContext _db;
    private readonly WorkspaceContext _workspaceContext;

    public WorkspaceLoader(AppDbContext db, WorkspaceContext workspaceContext)
    {
        _db = db;
        _workspaceContext = workspaceContext;
    }

    public async Task<Workspace?> LoadFromWorkspaceIdAsync(int workspaceId, CancellationToken ct = default)
    {
        var workspace = await _db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workspaceId, ct);
        if (workspace != null)
            _workspaceContext.SetMetadata(workspace);
        return workspace;
    }

    public async Task<Workspace?> LoadFromWebhookTokenAsync(string token, CancellationToken ct = default)
    {
        var workspace = await _db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.WebhookToken == token, ct);
        if (workspace != null)
            _workspaceContext.SetMetadata(workspace);
        return workspace;
    }

    public async Task<Workspace?> LoadFromIntegrationTokenAsync(string token, CancellationToken ct = default)
    {
        var workspace = await _db.Workspaces.AsNoTracking().FirstOrDefaultAsync(w => w.IntegrationToken == token, ct);
        if (workspace != null)
            _workspaceContext.SetMetadata(workspace);
        return workspace;
    }
}

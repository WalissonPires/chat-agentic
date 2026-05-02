using ChatAgentic.Entities;
using ChatAgentic.Features.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence;

public sealed class WorkflowLoader
{
    private readonly AppDbContext _db;
    private readonly WorkflowContext _workflowContext;

    public WorkflowLoader(AppDbContext db, WorkflowContext workflowContext)
    {
        _db = db;
        _workflowContext = workflowContext;
    }

    public async Task<Workflow?> LoadFromWebhookTokenAsync(string token, CancellationToken ct = default)
    {
        var workflow = await _db.Workflows.AsNoTracking().FirstOrDefaultAsync(w => w.WebhookToken == token, ct);
        if (workflow != null)
            _workflowContext.SetFromWorkflow(workflow);
        return workflow;
    }

    public async Task<Workflow?> LoadFromWorkflowIdAsync(int workflowId, CancellationToken ct = default)
    {
        var workflow = await _db.Workflows.AsNoTracking().FirstOrDefaultAsync(w => w.Id == workflowId, ct);
        if (workflow != null)
            _workflowContext.SetFromWorkflow(workflow);
        return workflow;
    }
}

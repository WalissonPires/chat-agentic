using ChatAgentic.Entities;
using ChatAgentic.Features.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence;

public sealed class AgentDefinitionLoader
{
    private readonly AppDbContext _db;
    private readonly WorkflowContext _workflowContext;

    public AgentDefinitionLoader(AppDbContext db, WorkflowContext workflowContext)
    {
        _db = db;
        _workflowContext = workflowContext;
    }

    public async Task<AgentDefinition?> LoadFromWebhookTokenAsync(string token, CancellationToken ct = default)
    {
        var agentDefinition = await _db.Agents.AsNoTracking().FirstOrDefaultAsync(w => w.WebhookToken == token, ct);
        if (agentDefinition != null)
            _workflowContext.SetFromAgentDefinition(agentDefinition);
        return agentDefinition;
    }

    public async Task<AgentDefinition?> LoadFromAgentDefinitionIdAsync(int agentDefinitionId, CancellationToken ct = default)
    {
        var agentDefinition = await _db.Agents.AsNoTracking().FirstOrDefaultAsync(w => w.Id == agentDefinitionId, ct);
        if (agentDefinition != null)
            _workflowContext.SetFromAgentDefinition(agentDefinition);
        return agentDefinition;
    }
}

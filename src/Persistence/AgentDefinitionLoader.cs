using ChatAgentic.Entities;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Workflows;
using Microsoft.EntityFrameworkCore;

namespace ChatAgentic.Persistence;

public sealed class AgentDefinitionLoader
{
    private readonly AppDbContext _db;
    private readonly WorkflowContext _workflowContext;
    private readonly ChannelContext _channelContext;

    public AgentDefinitionLoader(AppDbContext db, WorkflowContext workflowContext, ChannelContext channelContext)
    {
        _db = db;
        _workflowContext = workflowContext;
        _channelContext = channelContext;
    }

    public async Task<AgentDefinition?> LoadFromWebhookTokenAsync(string token, CancellationToken ct = default)
    {
        var agentDefinition = await _db.Agents.AsNoTracking()
            .Include(a => a.WhatsappChannel)
            .Include(a => a.TelegramChannel)
            .FirstOrDefaultAsync(w => w.WebhookToken == token, ct);

        if (agentDefinition != null)
        {
            _workflowContext.SetFromAgentDefinition(agentDefinition);
            _channelContext.SetFromAgentDefinition(agentDefinition);
        }

        return agentDefinition;
    }

    public async Task<AgentDefinition?> LoadFromAgentDefinitionIdAsync(int agentDefinitionId, CancellationToken ct = default)
    {
        var agentDefinition = await _db.Agents.AsNoTracking()
            .Include(a => a.WhatsappChannel)
            .Include(a => a.TelegramChannel)
            .FirstOrDefaultAsync(w => w.Id == agentDefinitionId, ct);

        if (agentDefinition != null)
        {
            _workflowContext.SetFromAgentDefinition(agentDefinition);
            _channelContext.SetFromAgentDefinition(agentDefinition);
        }

        return agentDefinition;
    }
}

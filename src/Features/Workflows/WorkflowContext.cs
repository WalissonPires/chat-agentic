using ChatAgentic.Entities;

namespace ChatAgentic.Features.Workflows;

public sealed class WorkflowContext
{
    private AgentDefinitionMetadata? _metadata;

    public AgentDefinitionMetadata Metadata => _metadata ?? throw new InvalidOperationException("WorkflowContext is not initialized. Call SetFromAgentDefinition before resolving workflow-scoped services.");

    public int AgentDefinitionId { get; private set; }
    public int WorkspaceId { get; private set; }

    public void SetFromAgentDefinition(AgentDefinition agentDefinition)
    {
        _metadata = agentDefinition.Metadata;
        AgentDefinitionId = agentDefinition.Id;
        WorkspaceId = agentDefinition.WorkspaceId;
    }
}

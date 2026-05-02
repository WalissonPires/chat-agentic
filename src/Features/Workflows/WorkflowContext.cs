using ChatAgentic.Entities;

namespace ChatAgentic.Features.Workflows;

public sealed class WorkflowContext
{
    private WorkflowMetadata? _metadata;

    public WorkflowMetadata Metadata => _metadata ?? throw new InvalidOperationException("WorkflowContext is not initialized. Call SetFromWorkflow before resolving workflow-scoped services.");

    public int WorkflowId { get; private set; }
    public int WorkspaceId { get; private set; }

    public void SetFromWorkflow(Workflow workflow)
    {
        _metadata = workflow.Metadata;
        WorkflowId = workflow.Id;
        WorkspaceId = workflow.WorkspaceId;
    }
}

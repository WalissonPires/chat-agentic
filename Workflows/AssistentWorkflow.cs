using ChatAgentic.Channels;
using Microsoft.Agents.AI.Workflows;

namespace ChatAgentic.Workflows
{
    public class AssistentWorkflow
    {
        private readonly ILogger _logger;
        private readonly LoadConversationExecutor _loadConversaion;
        private readonly SaveConversationExecutor _saveConversation;

        public AssistentWorkflow(ILogger<AssistentWorkflow> logger, LoadConversationExecutor loadConversaion, SaveConversationExecutor saveConversation)
        {
            _logger = logger;
            _loadConversaion = loadConversaion;
            _saveConversation = saveConversation;
        }

        public async Task RunAsync(Message message, CancellationToken ct = default)
        {
            var workflow = BuildWorkflow();

            _logger.LogInformation("Start workflow for {channel}: {identifier}", message.Channel, message.SenderIdentifier);

            var result = await InProcessExecution.RunStreamingAsync(workflow, message, null, ct);

            await foreach (var evt in result.WatchStreamAsync(ct))
            {
                switch (evt)
                {
                    case WorkflowOutputEvent outputEvt:
                        var output = outputEvt.Data?.ToString();
                        _logger.LogInformation("Workflow output: {Output}", output);
                        break;

                    case ExecutorInvokedEvent invokedEvt:
                        _logger.LogDebug("Executor started: {Id}", invokedEvt.ExecutorId);
                        break;

                    case ExecutorCompletedEvent completedEvt:
                        _logger.LogDebug("Executor completed: {Id}", completedEvt.ExecutorId);
                        break;

                    case WorkflowErrorEvent errorEvt:
                        _logger.LogError(errorEvt.Exception, "Workflow error: {Message}",
                            errorEvt.Exception?.Message);
                        break;
                    default:
                        _logger.LogDebug("Workflow event: {event}", evt.GetType().Name);
                        break;
                }
            }

            _logger.LogInformation("Workflow run completed for {channel}: {identifier}", message.Channel, message.SenderIdentifier);
        }

        private Workflow BuildWorkflow()
        {
            var workflow = new WorkflowBuilder(_loadConversaion)
                .AddEdge(_loadConversaion, _saveConversation)
                .Build();
            return workflow;
        }
    }
}
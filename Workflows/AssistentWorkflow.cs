using ChatAgentic.Channels;
using Microsoft.Agents.AI.Workflows;

namespace ChatAgentic.Workflows
{
    public class AssistentWorkflow
    {
        private readonly ILogger _logger;
        private readonly LoadContextExecutor _loadContext;
        private readonly SpeechToTextExecutor _speechToText;
        private readonly SaveConversationExecutor _saveConversation;
        private readonly AIAgentExecutor _aiAgent;
        private readonly ReplyMessgeExecutor _replyMessage;
        private readonly TextToSpeechExecutor _textToSpeech;

    public AssistentWorkflow(ILogger<AssistentWorkflow> logger, LoadContextExecutor loadContext, SpeechToTextExecutor speechToText,
        SaveConversationExecutor saveConversation, AIAgentExecutor aiAgent, ReplyMessgeExecutor replyMessage,
        TextToSpeechExecutor textToSpeech)
    {
      _logger = logger;
      _loadContext = loadContext;
      _saveConversation = saveConversation;
      _speechToText = speechToText;
      _aiAgent = aiAgent;
      _replyMessage = replyMessage;
      _textToSpeech = textToSpeech;
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
            var workflow = new WorkflowBuilder(_loadContext)
                .AddEdge<WorkflowExecutionContext>(_loadContext, _speechToText, weContext => weContext?.ReceiveidAudio == true)
                .AddEdge<WorkflowExecutionContext>(_loadContext, _aiAgent, weContext => weContext?.ReceiveidAudio != true)
                .AddEdge(_speechToText, _aiAgent)
                .AddEdge<WorkflowExecutionContext>(_aiAgent, _textToSpeech, weContext => weContext?.ReceiveidAudio == true)
                .AddEdge<WorkflowExecutionContext>(_aiAgent, _replyMessage, weContext => weContext?.ReceiveidAudio != true)
                .AddEdge(_textToSpeech, _replyMessage)
                .AddEdge(_replyMessage, _saveConversation)
                .Build();

            return workflow;
        }
    }
}
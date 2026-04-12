using System.Text.Json;
using ChatAgentic.Entities;
using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Agent;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Channels.Telegram;
using ChatAgentic.Features.Channels.Whatsapp;
using ChatAgentic.Features.Knowledgebase;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Features.Workflows.Executors;
using ChatAgentic.Features.Workspaces;
using ChatAgentic.Persistence;
using ChatAgentic.Queue;
using ChatAgentic.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.AddScoped<WorkspaceContext>();
builder.Services.AddScoped<WorkspaceLoader>();
builder.Services.AddScoped<AIProviderOptions>(sp =>
{
    var ctx = sp.GetRequiredService<WorkspaceContext>();
    return ctx.Metadata.AIProvider ?? throw new InvalidOperationException("AI provider is not configured on workspace metadata.");
});
builder.Services.AddScoped<EvolutionApiOptions>(sp =>
{
    var ctx = sp.GetRequiredService<WorkspaceContext>();
    return ctx.Metadata.EvolutionApi ?? throw new InvalidOperationException("EvolutionApi is not configured on workspace metadata.");
});
builder.Services.AddScoped<TelegramApiOptions>(sp =>
{
    var ctx = sp.GetRequiredService<WorkspaceContext>();
    return ctx.Metadata.Telegram ?? throw new InvalidOperationException("Telegram is not configured on workspace metadata.");
});
builder.Services.AddScoped<EvolutionApiClient>();
builder.Services.AddScoped<TelegramApiClient>();

builder.Services.AddScoped<ChannelMessageTransformFactory>();
builder.Services.AddScoped<WhatsappMessageTransform>();
builder.Services.AddScoped<TelegramMessageTransform>();
builder.Services.AddScoped<ChannelSendMessageFactory>();
builder.Services.AddScoped<WhatsappSendMessage>();
builder.Services.AddScoped<TelegramSendMessage>();
builder.Services.AddScoped<WebhookMessageProcessor>();

builder.Services.AddSingleton<IMessageQueue<Message>, InMemoryMessageQueue<Message>>();
builder.Services.AddTransient<AssistentWorkflow>();
builder.Services.AddHostedService<MessageConsumer>();
builder.Services.AddTransient<LoadContextExecutor>();
builder.Services.AddTransient<SaveConversationExecutor>();
builder.Services.AddTransient<SpeechToTextExecutor>();
builder.Services.AddTransient<AIAgentExecutor>();
builder.Services.AddTransient<TextToSpeechExecutor>();
builder.Services.AddTransient<ReplyMessgeExecutor>();

builder.Services.AddScoped<SpeechToTextService>();
builder.Services.AddScoped<TextToSpeechService>();
builder.Services.AddScoped<AIAgentFactory>();
builder.Services.AddSingleton<AIAgentToolsFactory>();
builder.Services.AddScoped<AIAgentSkillsFactory>();
builder.Services.AddTransient<MessageMediaStream>();
builder.Services.AddScoped<EmbeddingService>();
builder.Services.AddScoped<TextSearchAdpter>();
builder.Services.AddScoped<TextSearchProviderFactory>();

builder.Services.AddScoped<DocumentExtractor>();
builder.Services.AddScoped<KnowledgeBaseIngestor>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(
        builder.Configuration.GetValue<string>("ConnectionString"),
        x => x.UseVector()
    )
    .UseSnakeCaseNamingConvention();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/", () => new { Status = "healthy" });

app.MapPost("/webhook/{channel}/{token}", async (string channel, string token, JsonElement body, WebhookMessageProcessor messageProcessor) =>
{
    var channelType = Enum.Parse<ChannelType>(channel, ignoreCase: true);
    await messageProcessor.Execute(new(channelType, token, body.ToString()));
});

app.MapPost("/knowledge/ingestion", async ([FromForm] KnowledgeIngestionDTO dto, IServiceScopeFactory scopeFactory) =>
{
    if (dto.File == null)
        return Results.BadRequest(new { Message = "File is required" });

    await using var scope = scopeFactory.CreateAsyncScope();
    var sp = scope.ServiceProvider;

    var token = dto.Token ?? string.Empty;
    if (string.IsNullOrEmpty(token))
        return Results.BadRequest(new { Message = "Token is required" });

    var workspaceLoader = sp.GetRequiredService<WorkspaceLoader>();
    var workspace = await workspaceLoader.LoadFromIntegrationTokenAsync(token);
    if (workspace == null)
        return Results.BadRequest(new { Message = "Token invalid" });

    var ingestor = sp.GetRequiredService<KnowledgeBaseIngestor>();
    await ingestor.ExecuteAsync(new KnowledgeBaseIngestorInput(
        Context: dto.Context ?? Knowledge.DefaultContext,
        ChunkerType: dto.ChunkerType,
        ClearText: dto.ClearText ?? false,
        Token: token,
        Filename: dto.File.FileName,
        File: dto.File.OpenReadStream()
    ));

    return Results.Ok();
})
.DisableAntiforgery();

app.Run();
using System.Text.Json;
using ChatAgentic.Entities;
using ChatAgentic.Features.AI;
using ChatAgentic.Features.AI.Agent;
using ChatAgentic.Features.Channels;
using ChatAgentic.Features.Channels.Whatsapp;
using ChatAgentic.Features.Knowledgebase;
using ChatAgentic.Features.Workflows;
using ChatAgentic.Features.Workflows.Executors;
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


builder.Services.Configure<EvolutionApiOptions>(builder.Configuration.GetSection("EvolutionApi"));
builder.Services.AddScoped<EvolutionApiClient>();

builder.Services.AddScoped<ChannelMessageTransformFactory>();
builder.Services.AddScoped<WhatsappMessageTransform>();
builder.Services.AddScoped<ChannelSendMessageFactory>();
builder.Services.AddScoped<WhatsappSendMessage>();
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

builder.Services.Configure<AIProviderOptions>(builder.Configuration.GetSection("AIProvider"));
builder.Services.AddScoped<SpeechToTextService>();
builder.Services.AddScoped<TextToSpeechService>();
builder.Services.AddScoped<AIAgentFactory>();
builder.Services.AddScoped<AIAgentToolsFactory>();
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

app.UseHttpsRedirection();

app.MapGet("/", () => new { Status = "healthy" });

app.MapPost("/webhook/{channel}/{token}", async (string channel, string token, JsonElement body, WebhookMessageProcessor messageProcessor) =>
{
    var channelType = Enum.Parse<ChannelType>(channel, ignoreCase: true);
    await messageProcessor.Execute(new(channelType, token, body.ToString()));
});

app.MapPost("/knowledge/ingestion", async ([FromForm]KnowledgeIngestionDTO dto, KnowledgeBaseIngestor ingestor) =>
{
    if (dto.File == null)
        return Results.BadRequest(new { Message = "File is required" });

    await ingestor.ExecuteAsync(new KnowledgeBaseIngestorInput(
        Context: dto.Context ?? Knowledge.DefaultContext,
        ChunkerType: dto.ChunkerType,
        ClearText: dto.ClearText ?? false,
        Token: dto.Token ?? string.Empty,
        Filename: dto.File.FileName,
        File: dto.File.OpenReadStream()
    ));

    return Results.Ok();
})
.DisableAntiforgery();

app.Run();
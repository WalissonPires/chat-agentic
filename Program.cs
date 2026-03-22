using System.Text.Json;
using ChatAgentic.Channels;
using ChatAgentic.Data;
using ChatAgentic.Models;
using ChatAgentic.Services;
using ChatAgentic.Workflows;
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

public class KnowledgeIngestionDTO
{
    public string? Context { get; set; }
    public ChunkerType ChunkerType { get; set; }
    public bool? ClearText { get; set; }
    public string? Token { get; set; }
    public IFormFile? File { get; set; }
}
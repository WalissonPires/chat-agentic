using System.Text.Json;
using ChatAgentic.Channels;
using ChatAgentic.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddHttpClient();

builder.Services.Configure<EvolutionApiOptions>(builder.Configuration.GetSection("EvolutionApi"));
builder.Services.AddScoped<EvolutionApiClient>();

builder.Services.AddScoped<WhatsappMessageTransform>();
builder.Services.AddScoped<ChannelMessageTransformFactory>();
builder.Services.AddScoped<WebhookMessageProcessor>();

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetValue<string>("ConnectionString"))
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
    var channelType = Enum.Parse<ChannelType>(channel);
    await messageProcessor.Execute(new(channelType, token, body.ToString()));
});

app.Run();
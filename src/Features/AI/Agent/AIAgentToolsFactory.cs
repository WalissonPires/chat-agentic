using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ChatAgentic.Features.AI.Agent
{
    public sealed class AIAgentToolsFactory : IAsyncDisposable
    {
        private static readonly JsonSerializerOptions serializerOptions;

        static AIAgentToolsFactory()
        {
            serializerOptions = new JsonSerializerOptions()
            {
                PropertyNameCaseInsensitive = true,
            };

            serializerOptions.Converters.Add(new JsonStringEnumConverter());
        }

        private readonly ILoggerFactory _loggerFactory;
        private readonly SemaphoreSlim _initLock = new(1, 1);
        private List<McpClient>? _mcpClients;
        private List<AITool>? _cachedTools;
        private int _disposed;

        public AIAgentToolsFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task<List<AITool>> CreateAsync()
        {
            await _initLock.WaitAsync();
            try
            {
                ObjectDisposedException.ThrowIf(_disposed != 0, this);

                var logger = _loggerFactory.CreateLogger<AIAgentToolsFactory>();

                if (_cachedTools is not null)
                {
                    logger.LogDebug("Tools already loaded");
                    return _cachedTools;
                }

                logger.LogDebug("Loading agent tools");

                var tools = new List<AITool>();
                var clients = new List<McpClient>();
                var appToolsPath = Path.Combine(Directory.GetCurrentDirectory(), ".agent", "tools");

                if (!Directory.Exists(appToolsPath))
                {
                    logger.LogDebug("Tools path not exists: {path}", appToolsPath);
                    _mcpClients = clients;
                    _cachedTools = tools;
                    return _cachedTools;
                }

                var toolsFolders = Directory.GetDirectories(appToolsPath);
                logger.LogDebug("{count} tools folders found", toolsFolders.Length);

                foreach (var toolFolder in toolsFolders)
                {
                    logger.LogDebug("Process tool {path}", toolFolder);

                    var toolJsonFilename = Path.Combine(toolFolder, "TOOL.json");
                    if (!File.Exists(toolJsonFilename))
                    {
                        logger.LogDebug("Tool file not found: {filename}", toolJsonFilename);
                        continue;
                    }

                    var toolJson = await File.ReadAllTextAsync(toolJsonFilename);
                    if (string.IsNullOrEmpty(toolJson))
                    {
                        logger.LogDebug("Tool file is empty: {filename}", toolJsonFilename);
                        continue;
                    }

                    var toolMeta = JsonSerializer.Deserialize<AgentTool>(toolJson, serializerOptions);
                    if (toolMeta == null)
                    {
                        logger.LogDebug("Tool file is invalid: {filename}", toolJsonFilename);
                        continue;
                    }

                    if (!toolMeta.Enabled)
                    {
                        logger.LogDebug("Tool is inactive");
                        continue;
                    }

                    var toolName = Path.GetFileName(toolFolder);
                    if (string.IsNullOrEmpty(toolName))
                    {
                        logger.LogDebug("Tool name is empty");
                        continue;
                    }

                    if (!string.IsNullOrEmpty(toolMeta.WorkDir) && toolMeta.WorkDir.Contains(".."))
                        throw new Exception($"Tool {toolName} has invalid WorkDir");

                    IClientTransport transport = toolMeta.Type switch
                    {
                        AgentToolType.STDIO => new StdioClientTransport(new StdioClientTransportOptions
                        {
                            Name = toolName,
                            Command = toolMeta.Command ?? throw new Exception("Tool command is empy"),
                            WorkingDirectory = Path.Combine(toolFolder, toolMeta.WorkDir ?? string.Empty),
                            Arguments = toolMeta.Args,
                            EnvironmentVariables = toolMeta.Envs
                        }, _loggerFactory),
                        AgentToolType.SSE => new HttpClientTransport(new HttpClientTransportOptions
                        {
                            Name = toolName,
                            Endpoint = !string.IsNullOrEmpty(toolMeta.Endpoint) ? new Uri(toolMeta.Endpoint) : throw new Exception("Tool endpoint is empty"),
                            AdditionalHeaders = toolMeta.Headers
                        }, _loggerFactory),
                        _ => throw new NotSupportedException("Tool type invalid: " + toolMeta.Type),
                    };

                    var mcpClient = await McpClient.CreateAsync(transport);
                    clients.Add(mcpClient);

                    var mcpTools = await mcpClient.ListToolsAsync();
                    mcpTools = mcpTools.Select(tool => tool.WithName(new AgentToolFunctionName(toolName, tool.Name).GetFullName())).ToList();
                    tools.AddRange(mcpTools);

                    logger.LogInformation("Tool {name} registred", toolName);
                }

                _mcpClients = clients;
                _cachedTools = tools;
                return _cachedTools;
            }
            finally
            {
                _initLock.Release();
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
                return;

            await _initLock.WaitAsync();
            try
            {
                if (_mcpClients is not null)
                {
                    foreach (var client in _mcpClients)
                        await client.DisposeAsync();
                    _mcpClients = null;
                }

                _cachedTools = null;
            }
            finally
            {
                _initLock.Release();
                _initLock.Dispose();
            }
        }
    }

    public class AgentTool
    {
        public AgentToolType Type { get; set; }
        public bool Enabled { get; set; }

        public string? Command { get; set; }
        public string? WorkDir { get; set; }
        public string[]? Args { get; set; }
        public Dictionary<string, string?>? Envs { get; set; }

        public string? Endpoint { get; set; }
        public Dictionary<string, string>? Headers { get; set; }
    }

    public enum AgentToolType
    {
        STDIO = 1,
        SSE = 2
    }

    public class AgentToolFunctionName
    {
        private readonly string _mcp;
        private readonly string _tool;

        public string Mcp => _mcp;
        public string ToolName => _tool;

        public AgentToolFunctionName(string mcp, string tool)
        {
            _mcp = mcp;
            _tool = tool;
        }

        public string GetFullName()
        {
            return _mcp + "__" + _tool;
        }

        public static bool TryFromFullName(string fullname, out AgentToolFunctionName name)
        {
            name = default!;

            var split = fullname.Split("__", StringSplitOptions.RemoveEmptyEntries);
            if (split.Length != 2)
                return false;

            name = new AgentToolFunctionName(split[0], split[1]);
            return true;
        }
    }
}

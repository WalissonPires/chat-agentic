using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ChatAgentic.Features.AI.Agent
{
    public class AIAgentToolsFactory
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

        public AIAgentToolsFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public async Task<List<AITool>> CreateAsync()
        {
            var logger = _loggerFactory.CreateLogger<AIAgentToolsFactory>();

            logger.LogDebug("Loading agent tools");

            var tools = new List<AITool>();
            var appToolsPath = Path.Combine(Directory.GetCurrentDirectory(), ".agent", "tools");

            if (!Directory.Exists(appToolsPath))
            {
                logger.LogDebug("Tools path not exists: {path}", appToolsPath);
                return [];
            }

            var toolsFolders = Directory.GetDirectories(appToolsPath);
            logger.LogDebug("{count} tools folders found", toolsFolders.Length);

            foreach(var toolFolder in toolsFolders)
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
                var mcpTools = await mcpClient.ListToolsAsync();
                mcpTools = mcpTools.Select(tool => tool.WithName(new AgentToolFunctionName(toolName, tool.Name).GetFullName())).ToList();
                tools.AddRange(mcpTools);

                logger.LogInformation("Tool {name} registred", toolName);
            }

            return tools;
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
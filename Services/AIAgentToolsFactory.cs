using Microsoft.Extensions.AI;
using ModelContextProtocol.Client;

namespace ChatAgentic.Services
{
    public class AIAgentToolsFactory
    {
        private readonly ILoggerFactory _loggerFactory;
        private readonly Dictionary<string, AgentTool> _tools;

        public AIAgentToolsFactory(IConfiguration config, ILoggerFactory loggerFactory)
        {
            _tools = config.GetSection("Tools").Get<Dictionary<string, AgentTool>>() ?? [];
            _loggerFactory = loggerFactory;
        }

        public async Task<List<AITool>> CreateAsync()
        {
            var tools = new List<AITool>();
            var appToolsPath = Path.Combine(Directory.GetCurrentDirectory(), ".agent", "tools");

            foreach (var (key, value) in _tools.Where(x => x.Value.Enabled).ToArray())
            {
                IClientTransport transport = value.Type switch
                {
                    AgentToolType.Stdio => new StdioClientTransport(new StdioClientTransportOptions
                    {
                        Name = key,
                        Command = value.Command ?? throw new Exception("Tool command is empy"),
                        WorkingDirectory = Path.Combine(appToolsPath, key, value.WorkDir ?? string.Empty),
                        Arguments = value.Args,
                        EnvironmentVariables = value.Envs
                    }, _loggerFactory),
                    AgentToolType.SSE => new HttpClientTransport(new HttpClientTransportOptions
                    {
                        Name = key,
                        Endpoint = !string.IsNullOrEmpty(value.Endpoint) ? new Uri(value.Endpoint) : throw new Exception("Tool endpoint is empty"),
                        AdditionalHeaders = value.Headers
                    }, _loggerFactory),
                    _ => throw new NotSupportedException("Tool type invalid: " + value.Type),
                };

                var mcpClient = await McpClient.CreateAsync(transport);
                var mcpTools = await mcpClient.ListToolsAsync();
                mcpTools = mcpTools.Select(tool => tool.WithName(new AgentToolFunctionName(key, tool.Name).GetFullName())).ToList();
                tools.AddRange(mcpTools);
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
        Stdio = 1,
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
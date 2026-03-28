using Microsoft.Agents.AI;
using Microsoft.Extensions.AI;
using AgentsAIAgent = Microsoft.Agents.AI.AIAgent;

namespace ChatAgentic.Features.AI.Agent
{
    public class AIAgentMiddleware
    {
        public static async ValueTask<object?> InjectToolArguments(AgentsAIAgent agent, FunctionInvocationContext context,
            Func<FunctionInvocationContext, CancellationToken, ValueTask<object?>> next, CancellationToken cancellationToken)
        {
            var additionalProperties = context.Options?.AdditionalProperties ?? [];

            foreach(var prop in additionalProperties)
            {
                // prop key pattern: toolname.argument
                if (!AgentToolFunctionName.TryFromFullName(context.Function.Name, out var toolName))
                    continue;

                if (prop.Key.StartsWith(toolName.Mcp, StringComparison.InvariantCultureIgnoreCase))
                {
                    var argName = prop.Key.Replace(toolName.Mcp + '.', string.Empty, StringComparison.InvariantCultureIgnoreCase);
                    if (context.Arguments.ContainsKey(argName))
                    {
                        context.Arguments[argName] = prop.Value;
                    }
                }
            }

            var result = await next(context, cancellationToken);
            return result;
        }
    }
}
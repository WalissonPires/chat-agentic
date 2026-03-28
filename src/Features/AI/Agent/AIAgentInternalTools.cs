using System.ComponentModel;
using Microsoft.Extensions.AI;

namespace ChatAgentic.Features.AI.Agent
{
    public class AIAgentInternalTools
    {
        [Description("Get the current date and time in UTC ISO8601")]
        public static string GetCurrentDate() => DateTime.UtcNow.ToString("o");

        public static List<AITool> GetTools()
        {
            return
            [
                AIFunctionFactory.Create(AIAgentInternalTools.GetCurrentDate)
            ];
        }
    }
}
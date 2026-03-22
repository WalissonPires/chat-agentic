using Microsoft.Agents.AI;

namespace ChatAgentic.Services
{
    public class AIAgentSkillsFactory
    {
        private readonly ILoggerFactory _loggerFactory;

        public AIAgentSkillsFactory(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        public AIContextProvider Create()
        {
#pragma warning disable MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
            var skillsProvider = new FileAgentSkillsProvider(
                skillPath: Path.Combine(Directory.GetCurrentDirectory(), ".agent", "skills"),
                loggerFactory: _loggerFactory
            );
#pragma warning restore MAAI001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

            return skillsProvider;
        }
    }
}
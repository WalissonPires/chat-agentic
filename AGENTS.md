# AGENTS.md — Chat Agentic

## Commands & Development Setup
- **Run dependencies**: `docker compose up -d` (PostgreSQL with `pgvector` on port 15432)
- **Run API**: `dotnet run` (listens on port 5010 by default via `Properties/launchSettings.json`)
- **Build / Check**: `dotnet build` (`.NET 10`)
- **Swagger / OpenAPI**: `http://localhost:5010/swagger`
- **Hangfire Dashboard**: `http://localhost:5010/hangfire` (Development environment)

## Git & Commits
- **Commit Messages**: Must follow the Conventional Commits specification (e.g., `feat:`, `fix:`, `chore:`, `refactor:`, `docs:`).

## Architecture & Core Concepts
- **Multi-tenant Workspaces**: Everything is scoped by `workspace_id`. Credentials and AI settings live in `workspaces.metadata`.
- **Workflows & Webhooks**: Workflows define specific agent configurations and channel bindings (`EvolutionApi` for WhatsApp, `Telegram`), authenticated by a unique `webhook_token` at `/webhook/{channel}/{token}`.
- **Message Pipeline**: Webhook -> `InMemoryMessageQueue` -> `MessageConsumer` background worker -> `AssistantWorkflow` (LoadContext -> STT -> AIAgentExecutor with tools/skills/RAG -> TTS -> ReplyMessage -> SaveConversation).
- **Microsoft Agent Framework**: Uses `Microsoft.Agents.AI` and `Microsoft.Agents.AI.Workflows` packages.
- **Persistence**: PostgreSQL via Entity Framework Core (`AppDbContext`), `pgvector` extension for embeddings, and `EFCore.NamingConventions` (snake_case column mapping).

## Agent Extensibility (`.agent/`)
- **Skills**: Markdown instructions in `.agent/skills/<skill-name>/SKILL.md` (frontmatter with name, description, compatibility, metadata).
- **Tools**: MCP server configurations in `.agent/tools/<tool-name>/TOOL.json` supporting `SSE` or `STDIO` types.

## Structured Outputs (Audio + Text)
- When `Agent.UseStructuredOutput = true`, the agent returns JSON matching:
  ```json
  {
    "speakableText": "Natural language response without URLs (used for TTS)",
    "textSegments": ["https://example.com/url"]
  }
  ```
## Rules

- Nunca ler arquivos das pastas: node_modules, bin, obj.
- Seguir princípios SOLID e Clean Code em todo o código.
- Usar padrões modernos do C# 14 / .NET 10 (pattern matching, records, primary constructors).
- Seguir Conventional Commits para mensagens de commit (feat:, fix:, chore:, refactor:, docs:).
- Respeitar isolamento multi-tenant (sempre escopar por workspace_id).
- Programação assíncrona: sem sync-over-async.
- Investigar a estrutura do repositório antes de propor mudanças.
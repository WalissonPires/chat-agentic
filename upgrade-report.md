# Relatório de Upgrade - Microsoft Agent Framework

## Versão Atual vs. Mais Recente

| Pacote | Versão Atual | Versão Mais Recente | Versão Alvo |
|--------|-------------|---------------------|-------------|
| `Microsoft.Agents.AI` | **1.4.0** | **1.13.0** | **1.13.0** |
| `Microsoft.Agents.AI.OpenAI` | **1.4.0** | **1.13.0** | **1.13.0** |
| `Microsoft.Agents.AI.Workflows` | **1.4.0** | **1.13.0** | **1.13.0** |
| `ModelContextProtocol` | 1.1.0 | Verificar latest | Manter |

> **Nota:** O repositório já está em `net10.0`, compatível com v1.13.0. Nenhuma alteração de TFM necessária.

---

## 1. Atualização de Dependências (Obrigatório)

### Arquivo: `chat-agentic.csproj`

Atualizar as 3 referências do Agent Framework:

```xml
<!-- DE -->
<PackageReference Include="Microsoft.Agents.AI" Version="1.4.0" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.4.0" />
<PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.4.0" />

<!-- PARA -->
<PackageReference Include="Microsoft.Agents.AI" Version="1.13.0" />
<PackageReference Include="Microsoft.Agents.AI.OpenAI" Version="1.13.0" />
<PackageReference Include="Microsoft.Agents.AI.Workflows" Version="1.13.0" />
```

**Impacto:** A dependência transitiva `Microsoft.Extensions.AI` será atualizada automaticamente de uma versão anterior para `>= 10.6.0`.

---

## 2. Novas APIs Disponíveis (Melhorias)

### 2.1. Typed Executors (`Executor<TInput, TInput>`)

A versão 1.13.0 introduz `Executor<TInput, TOutput>` que elimina a necessidade de `ConfigureProtocol()` manual para casos típicos. Cada executor atual (`AIAgentExecutor`, `LoadContextExecutor`, `ReplyMessgeExecutor`, etc.) pode ser migrado.

**Migração por executor:**

| Executor | Arquivo | Atual | Recomendado |
|----------|---------|-------|-------------|
| `AIAgentExecutor` | `src/Features/Workflows/Executors/AIAgentExecutor.cs` | `Executor` (untyped) | `Executor<WorkflowExecutionContext, WorkflowExecutionContext>` |
| `LoadContextExecutor` | `src/Features/Workflows/Executors/LoadContextExecutor.cs` | `Executor` (untyped) | `Executor<Message, WorkflowExecutionContext>` |
| `ReplyMessgeExecutor` | `src/Features/Workflows/Executors/ReplyMessgeExecutor.cs` | `Executor` (untyped) | `Executor<WorkflowExecutionContext, WorkflowExecutionContext>` |
| `SaveConversationExecutor` | `src/Features/Workflows/Executors/SaveConversationExecutor.cs` | `Executor` (untyped) | `Executor<WorkflowExecutionContext, WorkflowExecutionContext>` |
| `SpeechToTextExecutor` | `src/Features/Workflows/Executors/SpeechToTextExecutor.cs` | `Executor` (untyped) | `Executor<WorkflowExecutionContext, WorkflowExecutionContext>` |
| `TextToSpeechExecutor` | `src/Features/Workflows/Executors/TextToSpeechExecutor.cs` | `Executor` (untyped) | `Executor<WorkflowExecutionContext, WorkflowExecutionContext>` |

**Exemplo de migração (LoadContextExecutor):**

```csharp
// ANTES (v1.4.0) - untyped com ConfigureProtocol manual
public sealed partial class LoadContextExecutor : Executor
{
    public LoadContextExecutor(...) : base("LoadContext") { }

    protected override ProtocolBuilder ConfigureProtocol(ProtocolBuilder protocolBuilder)
    {
        return protocolBuilder
            .SendsMessage<WorkflowExecutionContext>()
            .ConfigureRoutes(routes =>
            {
                routes.AddHandler<Message>(HandleAsync);
            });
    }

    public async ValueTask HandleAsync(Message message, IWorkflowContext context, CancellationToken ct) { ... }
}

// DEPOIS (v1.13.0) - typed executor
public sealed partial class LoadContextExecutor : Executor<Message, WorkflowExecutionContext>
{
    public LoadContextExecutor(...) : base("LoadContext") { }

    public override async ValueTask<WorkflowExecutionContext> HandleAsync(Message message, IWorkflowContext context, CancellationToken ct)
    {
        // ... mesma lógica ...
        return weContext; // return ao invés de SendMessageAsync
    }
}
```

**Benefício:** Elimina `ConfigureProtocol()`, `SendsMessage<T>()`, e `context.SendMessageAsync()` manual do handler. O framework gerencia o fluxo automaticamente.

> **ATENÇÃO:** Se migrar para typed executors, o `AssistentWorkflow.BuildWorkflow()` NÃO precisa de alteração, pois os edges são construídos por `ExecutorBinding` (que ambos os tipos suportam).

### 2.2. Fan-Out / Fan-In no WorkflowBuilder

Novo `AddFanOutEdge<T>()` e `AddFanInBarrierEdge()` disponíveis para workflows com processamento paralelo.

**Oportunidade para o projeto:**
- O `SpeechToTextExecutor` atual processa áudio em loop sequencial (`for` com `await`). Poderia usar fan-out para transcrever múltiplos áudios em paralelo.
- O `ReplyMessgeExecutor` envia mensagens em loop. Poderia usar fan-out para envio paralelo.

### 2.3. OpenTelemetry no WorkflowBuilder

```csharp
// NOVO: Habilitar telemetria nos workflows
var workflow = new WorkflowBuilder(_loadContext)
    .WithOpenTelemetry()  // NOVO
    .AddEdge<WorkflowExecutionContext>(...)
    .Build();
```

**Recomendação:** Adicionar `.WithOpenTelemetry()` em `AssistentWorkflow.BuildWorkflow()` para observabilidade.

### 2.4. AgentWorkflowBuilder - Novos Padrões

Novos métodos estáticos para padrões de workflow comuns:

```csharp
// Padrão sequencial (novo)
AgentWorkflowBuilder.BuildSequential(agent1, agent2, agent3);

// Padrão concurrent (novo)
AgentWorkflowBuilder.BuildConcurrent(agent1, agent2);

// Group chat (novo)
AgentWorkflowBuilder.CreateGroupChatBuilderWith(managerFactory);

// Handoff (novo)
AgentWorkflowBuilder.CreateHandoffBuilderWith(initialAgent);
```

**Oportunidade:** O `AssistentWorkflow` atual usa `WorkflowBuilder` manual. Para cenários futuros com múltiplos agentes, os builders especializados são mais simples.

### 2.5. CompactionProvider

Novo `CompactionProvider` registrado via `ChatClientAgentOptions.AIContextProviders`:

```csharp
// NOVO: Compaction de conversas longas
var chatAgentOptions = new ChatClientAgentOptions
{
    Name = "AI Assistent",
    ChatOptions = chatOptions,
    AIContextProviders = [
        new CompactionProvider(compactionPipeline) // NOVO
    ]
};
```

**Oportunidade:** O projeto atual mantém apenas as últimas 20 mensagens (`Take(20)` em `LoadContextExecutor.cs:105`). Um `CompactionProvider` poderia resumir conversas longas de forma inteligente ao invés de truncar.

### 2.6. AIAgent - CancelRunAsync e DeleteRunAsync

Novos métodos virtuais no `AIAgent`:

```csharp
public virtual Task<AgentResponse?> CancelRunAsync(string id, ...) { ... }
public virtual Task<AgentResponse?> DeleteRunAsync(string id, ...) { ... }
```

**Oportunidade:** Implementar cancelamento de execução em `AIAgentExecutor` para workflows de longa duração.

---

## 3. Breaking Changes a Verificar

### 3.1. Transitive Dependency `Microsoft.Extensions.AI` >= 10.6.0

A v1.13.0 depende de `Microsoft.Extensions.AI >= 10.6.0`. Verificar se algum código usa APIs antigas do `Microsoft.Extensions.AI` que foram alteradas:

| Namespace | Classes usadas no projeto | Status |
|-----------|--------------------------|--------|
| `Microsoft.Extensions.AI` | `ChatMessage`, `ChatRole`, `ChatOptions` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `AITool`, `AIFunctionFactory` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `TextContent`, `UriContent` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `FunctionCallContent`, `FunctionResultContent` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `UsageDetails` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `IEmbeddingGenerator` | Estável - sem impacto |
| `Microsoft.Extensions.AI` | `ChatResponseFormat.ForJsonSchema<T>()` | Estável - sem impacto |

### 3.2. AgentSkillsProvider (MAAI001)

O `AgentSkillsProvider` ainda está marcado como experimental (`MAAI001`). Verificar se na v1.13.0 essa warning foi resolvida ou se continua sendo suprimida em `AIAgentSkillsFactory.cs:16`.

### 3.3. `AgentRunOptions.AdditionalProperties`

O uso de `AdditionalProperties` em `AgentRunOptions` (`AIAgentExecutor.cs:58`) deve ser verificado contra a API da v1.13.0.

---

## 4. Arquivos que Requerem Alteração

### Obrigatório (Upgrade básico)

| # | Arquivo | Alteração | Prioridade |
|---|---------|-----------|------------|
| 1 | `chat-agentic.csproj` | Atualizar versões dos 3 pacotes para 1.13.0 | **Alta** |

### Recomendado (Melhorias pós-upgrade)

| # | Arquivo | Alteração | Prioridade |
|---|---------|-----------|------------|
| 2 | `src/Features/Workflows/AssistentWorkflow.cs` | Adicionar `.WithOpenTelemetry()` | Média |
| 3 | `src/Features/Workflows/Executors/*.cs` | Migrar para `Executor<TInput, TOutput>` (6 arquivos) | Média |
| 4 | `src/Features/AI/Agent/AIAgentFactory.cs` | Avaliar `CompactionProvider` | Baixa |
| 5 | `src/Features/Workflows/Executors/SpeechToTextExecutor.cs` | Avaliar fan-out para transcrição paralela | Baixa |

---

## 5. Plano de Execução Recomendado

### Fase 1 - Upgrade Básico (sem breaking changes)

1. Atualizar versões no `.csproj`
2. Executar `dotnet restore`
3. Executar `dotnet build` e verificar erros de compilação
4. Verificar warnings do analyzer
5. Rodar testes existentes (se houver)

### Fase 2 - Adotar Novas APIs

1. Migrar executors para typed (`Executor<TInput, TOutput>`)
2. Adicionar OpenTelemetry ao workflow
3. Avaliar CompactionProvider para conversas longas

### Fase 3 - Novos Recursos

1. Avaliar fan-out para processamento paralelo (STT)
2. Avaliar `AgentWorkflowBuilder` para workflows multi-agente
3. Implementar `CancelRunAsync` para cancelamento de execução

---

## 6. Riscos e Mitigações

| Risco | Probabilidade | Impacto | Mitigação |
|-------|--------------|---------|-----------|
| Breaking change em `AdditionalProperties` | Baixa | Alto | Verificar release notes da v1.13.0 |
| `AgentSkillsProvider` removido/alterado | Baixa | Alto | Verificar se MAAI001 persiste |
| `Microsoft.Extensions.AI` incompatível | Baixa | Médio | Testar compilação após upgrade |
| Typed executor quebra lógica existente | Baixa | Médio | Migrar um executor por vez e testar |

---

## 7. Referências

- [NuGet: Microsoft.Agents.AI 1.13.0](https://www.nuget.org/packages/Microsoft.Agents.AI/1.13.0)
- [Microsoft Agent Framework v1.0 Release](https://devblogs.microsoft.com/agent-framework/microsoft-agent-framework-version-1-0/)
- [Building Blocks for AI Part 3](https://devblogs.microsoft.com/dotnet/microsoft-agent-framework-building-blocks-for-ai-part-3/)
- [Upgrade Guides](https://learn.microsoft.com/en-us/agent-framework/support/upgrade)
- [GitHub: microsoft/agent-framework](https://github.com/microsoft/agent-framework)

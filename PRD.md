# Product Requirements Document — Chat Agentic

> **Versão:** 1.0  
> **Última atualização:** 2026-08-06  
> **Status:** Em desenvolvimento  

---

## 1. Visão Geral do Produto

### 1.1. Objetivo

Chat Agentic é uma **API REST para atendimento conversacional omnichannel com agente de IA**. O sistema recebe mensagens de usuários por canais integrados (WhatsApp, Telegram), processa cada interação em um pipeline orientado a workflow e devolve a resposta pelo mesmo canal, mantendo continuidade de conversa e acesso a base de conhecimento contextual.

### 1.2. Problema que Resolve

Empresas precisam de um ponto único de orquestração para atendimento automatizado em múltiplos canais, com:
- Contexto conversacional persistente por pessoa (não por canal)
- Base de conhecimento segmentada por domínio
- Agente de IA extensível com tools, skills e RAG
- Envio proativo de notificações agendadas
- Isolamento multi-tenant por workspace

### 1.3. Público-Alvo

- Desenvolvedores e empresas que precisam de uma API de atendimento IA omnichannel
- Operadores que gerenciam múltiplos tenants/clientes em uma única instância
- Integradores que conectam canais de mensageria (WhatsApp, Telegram) a um backend de IA

### 1.4. Princípios de Design

| Princípio | Descrição |
|-----------|-----------|
| **Multi-tenant por Workspace** | Cada workspace é um ambiente isolado — pessoas, contatos, knowledge, agentes e notificações pertencem a um único workspace |
| **Webhook-first** | Mensagens entram por webhook, são enfileiradas e processadas assincronamente |
| **Extensibilidade** | O agente é extensível via Tools (MCP), Skills (Markdown), RAG (pgvector) e Context Providers |
| **Omnichannel** | Uma pessoa pode ter contatos em múltiplos canais; a conversa é unificada por perfil |

---

## 2. Stack Tecnológica

| Área | Tecnologia |
|------|------------|
| Runtime e API | C#, **.NET 10**, ASP.NET Core |
| Agentes e Workflows | **Microsoft Agent Framework** (`Microsoft.Agents.AI.*` — OpenAI, Workflows) |
| Persistência | **PostgreSQL** com **Entity Framework Core**; extensão **pgvector** para embeddings e busca vetorial |
| Jobs/Notificações | **Hangfire** (agendamento recorrente e Jobs) com storage PostgreSQL |
| Canais | Webhooks por tipo de canal; **Evolution API** (WhatsApp) e **Telegram Bot API** |
| Fila interna | Fila em memória (`InMemoryMessageQueue`) com background consumer |
| Autenticação | Bearer Token por `IntegrationToken` do workspace |

---

## 3. Arquitetura

### 3.1. Fluxo de Mensagem

```
Canal (WhatsApp/Telegram)
    │
    ▼
POST /webhook/{channel}/{token}  ──▶  WebhookMessageProcessor
    │                                      │
    │                                      ▼
    │                              Fila (InMemoryMessageQueue)
    │                                      │
    │                                      ▼
    │                              MessageConsumer (Background)
    │                                      │
    │                                      ▼
    │                              AssistentWorkflow (Pipeline)
    │                                      │
    │                    ┌─────────────────┼──────────────────┐
    │                    ▼                 ▼                  ▼
    │              LoadContext       SpeechToText       (skip STT)
    │                    │                 │                  │
    │                    ▼                 ▼                  │
    │              AIAgentExecutor  ◄──────┘                  │
    │                    │                                    │
    │           ┌────────┼────────┐                           │
    │           ▼                 ▼                           │
    │     TextToSpeech     (skip TTS)                        │
    │           │                 │                           │
    │           ▼                 ▼                           │
    │        ReplyMessage  ◄─────┘◄──────────────────────────┘
    │              │
    │              ▼
    │       SaveConversation
    │
    ▼
Resposta no canal original
```

### 3.2. Autenticação

A API usa autenticação por **Bearer Token** (`IntegrationToken`). Cada workspace possui um `IntegrationToken` único. Ao receber uma requisição, o `IntegrationTokenAuthHandler` valida o token contra o banco e injeta `WorkspaceId` e `WorkspaceName` como claims no `ClaimsPrincipal`.

### 3.3. Modelo de Dados

```
Workspace (tenant)
├── Channel (WhatsApp, Telegram — credenciais)
├── AgentDefinition (webhook_token, instruções, canais associados)
├── Person (nome, metadados)
│   └── Contact (canal + identificador)
├── Knowledge (source, context, content, embedding)
├── Conversation (canal, sender, agente)
│   └── ConversationMessage (role, content, media)
├── NotificationRule (agenda, template, alvos)
│   └── NotificationLog (status, batch, período)
└── AIUsageHistory (provider, serviço, tokens, custo)
```

---

## 4. Features

### Legenda de Status

| Símbolo | Significado |
|---------|-------------|
| ✅ | Implementado e funcional |
| 🔧 | Parcialmente implementado — precisa de complemento |
| ❌ | Não implementado |
| 🚫 | Fora de escopo / Não deve ter endpoint por decisão de design |

---

### 4.1. Workspace (Tenant)

> Workspaces representam contas/tenants. São criados e gerenciados **internamente** — não possuem e não devem possuir endpoints de API.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Workspace` com `Name`, `IntegrationToken`, `Metadata` | ✅ | `src/Entities/Workspace.cs` |
| Metadata contém configuração do provedor de IA (`AIProviderOptions`) | ✅ | `ApiKey`, `Endpoint`, `ChatModel`, `EmbedModel`, `TranscriptionModel`, `TtsModel`, `TtsVoice`, `ImageModel` |
| Isolamento multi-tenant em todas as queries | ✅ | Filtro por `WorkspaceId` em todas as operações |
| Autenticação por `IntegrationToken` (Bearer) | ✅ | `IntegrationTokenAuthHandler` valida token e injeta claims |
| Workspace carregado e cacheado no escopo do request | ✅ | `WorkspaceContext` + `WorkspaceLoader` |
| CRUD de Workspace via API | 🚫 | Decisão de design: gerenciado internamente |

---

### 4.2. Channels (Canais de Comunicação)

> Canais representam as conexões com provedores de mensageria (Evolution API para WhatsApp, Telegram Bot API). Cada canal possui suas credenciais de acesso.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Channel` com `Name`, `Type`, `Credentials` | ✅ | `src/Entities/Channel.cs` |
| Tipos suportados: `Whatsapp`, `Telegram` | ✅ | `ChannelType` enum |
| Credenciais para Evolution API (`ServerUrl`, `ApiKey`, `Instance`) | ✅ | `EvolutionApiOptions` |
| Credenciais para Telegram (`BotToken`, `BaseUrl`, `FileBaseUrl`) | ✅ | `TelegramApiOptions` |
| Mapping EF Core com `Credentials` como JSON | ✅ | `ChannelMapping.cs` |
| **`POST /channels`** — criar canal | ❌ | |
| **`GET /channels`** — listar canais do workspace | ❌ | |
| **`GET /channels/{id}`** — obter canal por ID | ❌ | |
| **`PUT /channels/{id}`** — atualizar canal (credenciais) | ❌ | |
| **`DELETE /channels/{id}`** — deletar canal | ❌ | |

---

### 4.3. Agent Definitions (Agentes/Workflows)

> Cada agente define um ponto de entrada de webhook (`webhook_token`), instruções de sistema, e associação com canais. Um workspace pode ter múltiplos agentes (ex: dois números de WhatsApp diferentes).

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `AgentDefinition` com `Name`, `WebhookToken`, `Metadata` | ✅ | `src/Entities/AgentDefinition.cs` |
| Associação opcional com canal WhatsApp e Telegram | ✅ | `WhatsappChannelId`, `TelegramChannelId` |
| Metadata contém `AgentOptions` (instructions, flags de features) | ✅ | `AgentDefinitionMetadata` |
| `AgentOptions` flags: `UseStructuredOutput`, `EnableTools`, `EnableContextProviders`, `EnableAgentMiddleware`, `StrictToolNameValidation` | ✅ | `src/Features/AI/Agent/AgentOptions.cs` |
| Loader por `webhook_token` | ✅ | `AgentDefinitionLoader.LoadFromWebhookTokenAsync` |
| **`POST /agents`** — criar agente | ❌ | |
| **`GET /agents`** — listar agentes do workspace | ❌ | |
| **`GET /agents/{id}`** — obter agente por ID | ❌ | |
| **`PUT /agents/{id}`** — atualizar agente (instruções, canais, opções) | ❌ | |
| **`DELETE /agents/{id}`** — deletar agente | ❌ | |

---

### 4.4. People (Pessoas)

> Pessoas representam os usuários que conversam com os agentes. Uma mesma pessoa pode ter contatos em múltiplos canais. Metadados customizáveis são usados como filtros em notificações dinâmicas.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Person` com `Name`, `Metadata` (lista de key-value) | ✅ | `src/Entities/Person.cs` |
| Relação com `Workspace` (FK) | ✅ | `WorkspaceId` |
| Relação com `Contact` (1:N) | ✅ | `Contacts` navigation |
| Criação automática pelo webhook quando nova pessoa é detectada | ✅ | Via `WebhookMessageProcessor` → `LoadContextExecutor` |
| **`POST /people`** — criar pessoa manualmente | ❌ | |
| **`GET /people`** — listar pessoas do workspace (com paginação) | ❌ | Necessário para configurar notificações com `target_type: Specific` |
| **`GET /people/{id}`** — obter pessoa por ID (com contatos) | ❌ | |
| **`PUT /people/{id}`** — atualizar pessoa (nome, metadados) | ❌ | Necessário para filtros dinâmicos de notificação |
| **`DELETE /people/{id}`** — deletar pessoa | ❌ | |

---

### 4.5. Contacts (Contatos)

> Contatos associam uma pessoa a um identificador em um canal específico (ex: número de telefone no WhatsApp, username no Telegram). Isso permite conversa unificada multi-canal.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Contact` com `Channel` (tipo) e `Identifier` | ✅ | `src/Entities/Contact.cs` |
| Relação com `Person` (FK) | ✅ | `PersonId` |
| Criação automática pelo webhook | ✅ | Via `LoadContextExecutor` |
| **`POST /people/{personId}/contacts`** — adicionar contato | ❌ | |
| **`GET /people/{personId}/contacts`** — listar contatos | ❌ | |
| **`GET /people/{personId}/contacts/{id}`** — obter contato | ❌ | |
| **`PUT /people/{personId}/contacts/{id}`** — atualizar contato | ❌ | |
| **`DELETE /people/{personId}/contacts/{id}`** — deletar contato | ❌ | |

---

### 4.6. Webhook e Processamento de Mensagens

> Ponto de entrada HTTP para recebimento de mensagens dos canais. O webhook valida o token, transforma a mensagem para formato interno e enfileira para processamento assíncrono.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| **`POST /webhook/{channel}/{token}`** — entrada de mensagens | ✅ | `WebhookController` |
| Autenticação por `webhook_token` do agente | ✅ | Via `AgentDefinitionLoader` |
| Transform de mensagem por canal (WhatsApp, Telegram) | ✅ | `ChannelMessageTransformFactory` → `WhatsappMessageTransform`, `TelegramMessageTransform` |
| Sanitização de texto (remoção de caracteres invisíveis, limite 200 grafemas) | ✅ | `WebhookMessageProcessor.TextSanatization` |
| Enfileiramento em fila in-memory | ✅ | `InMemoryMessageQueue<Message>` |
| Consumer background para processamento | ✅ | `MessageConsumer` (HostedService) |
| Suporte a tipos de conteúdo: Texto, Áudio, Imagem, Vídeo, Documento | ✅ | `MessageContentType` enum |
| Rate limit de mensagens por usuário/canal | ❌ | |

---

### 4.7. Workflow de Processamento (Pipeline)

> O pipeline de processamento segue um grafo de executores encadeados, usando o Microsoft Agent Framework. Cada executor realiza uma etapa específica, com branching condicional baseado no contexto.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Pipeline via `WorkflowBuilder` (Microsoft Agent Framework) | ✅ | `AssistentWorkflow.BuildWorkflow()` |
| **LoadContextExecutor** — carrega workspace, agente, conversa, pessoa, contato | ✅ | `LoadContextExecutor.cs` |
| **SpeechToTextExecutor** — transcrição de áudio (condicional: `ReceiveidAudio`) | ✅ | `SpeechToTextExecutor.cs` |
| **AIAgentExecutor** — execução do agente de IA com tools, skills e RAG | ✅ | `AIAgentExecutor.cs` |
| **TextToSpeechExecutor** — síntese de voz (condicional: `ReceiveidAudio`) | ✅ | `TextToSpeechExecutor.cs` |
| **ReplyMessageExecutor** — envio de resposta pelo canal original | ✅ | `ReplyMessgeExecutor.cs` |
| **SaveConversationExecutor** — persistência do histórico da conversa | ✅ | `SaveConversationExecutor.cs` |
| Branching condicional (áudio → STT → AI → TTS → Reply / texto → AI → Reply) | ✅ | `AddEdge` com predicados no `WorkflowExecutionContext` |
| Logging de eventos do workflow (output, start, complete, error) | ✅ | Via `WatchStreamAsync` |

---

### 4.8. Agente de IA

> O agente de IA processa as mensagens recebidas, usando LLM com suporte a tools (MCP), skills (Markdown), busca semântica (RAG), saída estruturada e middleware.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Factory de agentes com opções configuráveis por `AgentDefinition` | ✅ | `AIAgentFactory.cs` |
| Provedor de IA configurável por workspace (OpenAI-compatible) | ✅ | `AIProviderOptions` no `WorkspaceMetadata` |
| **Tools (MCP)** — integração com Model Context Protocol | ✅ | `AIAgentToolsFactory.cs` |
| Suporte a MCP SSE e MCP STDIO | ✅ | Configuração via `TOOL.json` |
| **Skills** — instruções em Markdown carregadas do disco (`.agent/skills/`) | ✅ | `AIAgentSkillsFactory.cs` |
| Carregamento automático de skills e tools da pasta `.agent/` | ✅ | Scan de diretórios ao iniciar |
| **RAG** — busca textual e vetorial na base de conhecimento | ✅ | `TextSearchAdpter`, `TextSearchProviderFactory` |
| Segmentação da busca por **contexto** | ✅ | Filtragem por `Knowledge.Context` |
| **Saída estruturada** (JSON: `speakableText` + `textSegments`) | ✅ | `AgentStructuredResponse.cs` |
| Fallback para texto plano quando JSON inválido | ✅ | Tratamento no `AgentStructuredResponse` |
| **Middleware** de agente (pré/pós processamento) | ✅ | `AIAgentMiddleware.cs` |
| **Context Providers** (injeção de contexto adicional) | ✅ | Flag `EnableContextProviders` |
| Validação estrita de nomes de tools | ✅ | Flag `StrictToolNameValidation` |
| Configurar quais grupos de Tools/MCPs utilizar por agente | ❌ | Adicionar `Tools[]` e `Mcps[]` no `AgentOptions` |

---

### 4.9. Serviços de IA

> Serviços auxiliares de IA usados pelo pipeline: embeddings, speech-to-text, text-to-speech.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| **EmbeddingService** — geração de embeddings para RAG | ✅ | `EmbeddingService.cs` |
| **SpeechToTextService** — transcrição de áudio | ✅ | `SpeechToTextService.cs` |
| **TextToSpeechService** — síntese de voz | ✅ | `TextToSpeechService.cs` |
| Modelos configuráveis por workspace (`EmbedModel`, `TranscriptionModel`, `TtsModel`, `TtsVoice`) | ✅ | Via `AIProviderOptions` |

---

### 4.10. Envio de Respostas por Canal

> Envio de mensagens de volta ao usuário pelo canal original.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Factory de envio por canal | ✅ | `ChannelSendMessageFactory` |
| **WhatsApp** — envio via Evolution API | ✅ | `WhatsappSendMessage`, `EvolutionApiClient` |
| **Telegram** — envio via Telegram Bot API | ✅ | `TelegramSendMessage`, `TelegramApiClient` |
| Suporte a envio de texto, áudio e segmentos (URLs) | ✅ | Via `IChannelSendMessage.Execute` |

---

### 4.11. Conversas (Conversations)

> Conversas representam uma sessão de interação entre uma pessoa e um agente em um canal específico. Possuem expiração temporal.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Conversation` com `Channel`, `SenderIdentifier`, `ChatId`, `ExpireAt` | ✅ | `src/Entities/Conversation.cs` |
| Relação com `AgentDefinition` e `Workspace` | ✅ | `AgentId`, `WorkspaceId` |
| Entidade `ConversationMessage` com `Role`, `ContentType`, `ContentText`, `MediaUri` | ✅ | `src/Entities/ConversationMessage.cs` |
| Suporte a tipos: Text, Audio, Image, Video, Document | ✅ | `MessageContentType` enum |
| Mapeamento `ConversationMessage` ↔ `ChatMessage` (Microsoft.Extensions.AI) | ✅ | `MapToChatMessage`, `MapToConversationMessage` |
| Formato de `MessageId`: `{id}:{contentIndex}` (2 dígitos) | ✅ | Ex: `aG21a1c81fc2:00` |
| Persistência automática pelo `SaveConversationExecutor` | ✅ | No final do pipeline |
| Criação/recuperação automática de conversa pelo `LoadContextExecutor` | ✅ | Busca conversa ativa ou cria nova |
| **`GET /conversations`** — listar conversas (com filtros) | ❌ | |
| **`GET /conversations/{id}`** — obter conversa com mensagens | ❌ | |
| Delete/expirar conversa via API | 🚫 | Gerenciado internamente por expiração temporal |

---

### 4.12. Knowledge Base (Base de Conhecimento / RAG)

> A base de conhecimento permite ingestão de documentos, chunking, embedding e busca vetorial. O conteúdo é segmentado por **contexto** lógico dentro de cada workspace.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `Knowledge` com `Context`, `Source`, `Content`, `Embedding` (pgvector) | ✅ | `src/Entities/Knowledge.cs` |
| Contexto padrão (`default`) para documentos sem contexto explícito | ✅ | `Knowledge.DefaultContext` |
| **`POST /knowledge/ingestion`** — upload de arquivo (multipart) | ✅ | `KnowledgeController.Ingest` |
| `KnowledgeBaseIngestor` — pipeline de ingestão (extrair, chunkar, embedar, persistir) | ✅ | `KnowledgeBaseIngestor.cs` |
| `DocumentExtractor` — extração de texto de arquivos | ✅ | `DocumentExtractor.cs` |
| `TextCleaner` — limpeza de texto extraído | ✅ | `TextCleaner.cs` |
| Chunkers: `CharacterChunker`, `LineBreakChunker`, `MarkdownChunker`, `CsvChunker` | ✅ | Seleção por `ChunkerType` |
| Opção `ClearText` na ingestão | ✅ | Flag no `KnowledgeIngestionDTO` |
| Busca vetorial integrada ao agente (RAG) | ✅ | `TextSearchAdpter`, `EmbeddingService` |
| **`GET /knowledge`** — listar sources/documentos ingeridos (agrupado por source e context) | ❌ | |
| **`GET /knowledge/search?q=...&context=...`** — busca semântica pública (debug) | ❌ | |
| **`DELETE /knowledge/source/{sourceName}`** — remover chunks por source | ❌ | |
| **`DELETE /knowledge/context/{contextName}`** — remover chunks por contexto | ❌ | |
| **`DELETE /knowledge/{id}`** — remover chunk por ID | ❌ | |
| Suporte a ingestão de imagem (descrição via Vision model) | ❌ | |

---

### 4.13. Notification Rules (Sistema de Notificações)

> Sistema de notificações permite agendar e enviar mensagens proativas para pessoas do workspace, com suporte a múltiplos canais, templates com tags dinâmicas, alvos flexíveis e agendamento recorrente via Hangfire.

#### 4.13.1. CRUD de Regras de Notificação

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `NotificationRule` com `Name`, `Channels`, `MessageTemplate`, `Frequency`, `TargetType`, etc. | ✅ | `src/Entities/NotificationRule.cs` |
| **`POST /notifications`** — criar regra | ✅ | `NotificationRulesController.Create` |
| **`GET /notifications`** — listar regras do workspace | ✅ | `NotificationRulesController.List` |
| **`GET /notifications/{id}`** — obter regra por ID | ✅ | `NotificationRulesController.GetById` |
| **`PUT /notifications/{id}`** — atualizar regra | ✅ | `NotificationRulesController.Update` |
| **`DELETE /notifications/{id}`** — deletar regra (remove job Hangfire) | ✅ | `NotificationRulesController.Delete` |
| **`POST /notifications/{id}/trigger`** — disparo manual | ✅ | `NotificationRulesController.Trigger` |
| Service layer com interface `INotificationRuleService` | ✅ | `NotificationRuleService.cs` |

#### 4.13.2. Templates com Tags Dinâmicas

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Tag `{{person.name}}` — nome da pessoa | ✅ | `NotificationTagReplacer` |
| Tag `{{person.meta.<chave>}}` — metadado da pessoa | ✅ | |
| Tag `{{workspace.name}}` — nome do workspace | ✅ | |
| Tag `{{date}}` — data atual (`dd/MM/yyyy`) | ✅ | |
| Tag `{{time}}` — hora atual (`HH:mm`) | ✅ | |

#### 4.13.3. Alvos de Notificação

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| `All` — todas as pessoas do workspace | ✅ | `NotificationTargetType.All` |
| `Specific` — lista fixa de IDs de pessoas | ✅ | `NotificationTargetType.Specific` + `TargetPersonIds` |
| `Dynamic` — filtros compostos de metadados | ✅ | `NotificationTargetType.Dynamic` + `TargetFilters` |
| Operadores de filtro: `Equals`, `NotEquals`, `DayOfMonthWithin` | ✅ | `NotificationFilterOperator` |
| Estratégias de filtro: por nome, ID, metadados | ✅ | `IPersonFilterStrategy` implementations |
| Resolução de pessoas-alvo | ✅ | `NotificationPersonResolver` |

#### 4.13.4. Resolução de Canal (Omnichannel com Prioridade)

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Lista ordenada de canais preferenciais na regra | ✅ | `Channels: List<ChannelType>` |
| Resolução do melhor canal por contato disponível da pessoa | ✅ | `NotificationChannelResolver` |

#### 4.13.5. Agendamento e Execução

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Triggers: `Daily`, `Monthly`, `Yearly` | ✅ | `NotificationFrequency` enum |
| Cálculo de próxima execução | ✅ | `NextExecutionCalculator` |
| Sincronização com Hangfire (criar/atualizar/remover jobs) | ✅ | `NotificationSchedulerSync` |
| Job Hangfire para execução automática | ✅ | `NotificationHangfireJob` |
| Disparo e despacho de notificações | ✅ | `NotificationDispatcher` |
| Dashboard Hangfire em desenvolvimento (`/hangfire`) | ✅ | Ativado quando `ASPNETCORE_ENVIRONMENT=Development` |

#### 4.13.6. Deduplicação e Auditoria

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `NotificationLog` com `Status`, `ExecutionBatchId`, `ExecutionPeriodKey`, `SentAt` | ✅ | `src/Entities/NotificationLog.cs` |
| Status de log: `Sent`, `Failed`, `Skipped` | ✅ | `NotificationLogStatus` enum |
| Deduplicação por `ExecutionPeriodKey` (1 notificação/pessoa/período) | ✅ | Verificação no `NotificationDispatcher` |
| `ExecutionBatchId` para agrupamento de auditoria | ✅ | Gerado por disparo |
| **`GET /notifications/{ruleId}/logs`** — listar logs por regra | ❌ | |
| **`GET /notifications/logs?batchId={guid}`** — listar logs por batch | ❌ | |
| **`GET /people/{personId}/notification-logs`** — listar logs por pessoa | ❌ | |

---

### 4.14. AI Usage History (Histórico de Uso de IA)

> Registro de consumo de tokens/custo por serviço de IA, para auditoria e controle de gastos.

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| Entidade `AIUsageHistory` com `Provider`, `Service`, `Input`, `Output`, `Cost` | ✅ | `src/Entities/AIUsageHistory.cs` |
| Enum de serviços: Chat, Embedding, STT, TTS, etc. | ✅ | `AIUsageService` |
| Repositório para persistência (`IAIUsageHistoryRepository`) | ✅ | `AIUsageHistoryRepository.cs` |
| Factory para criação de registros | ✅ | `AIUsageHistoryFactory.cs` |
| Mapeamento de tokens por provider | ✅ | `AIUsageTokenMapper.cs` |
| Reports: `ChatUsageReport`, `EmbeddingAggregateUsageReport`, `SpeechToTextResult`, `EmbeddingResult` | ✅ | Interfaces `IAIUsageReport` |
| Registro via API | 🚫 | Escrita interna pelo pipeline — não deve ter endpoint de criação |
| Captura de tokens no `AIAgentExecutor` (Chat) | ❌ | Integração pendente |
| Captura de tokens no `KnowledgeBaseIngestor` (Embedding) | ❌ | Integração pendente |
| Captura de tokens no `SpeechToTextExecutor` (STT) | ❌ | Integração pendente |
| Captura de tokens no `TextToSpeechExecutor` (TTS) | ❌ | Integração pendente |
| **`GET /ai-usage`** — listar histórico de uso (com filtros) | ❌ | |
| **`GET /ai-usage/summary`** — consolidado por período | ❌ | |

---

### 4.15. Infraestrutura e DevOps

| Requisito | Status | Detalhes |
|-----------|--------|----------|
| `GET /` — health check | ✅ | Retorna `{ Status: "healthy" }` |
| Docker: `Dockerfile` para build e deploy | ✅ | `Dockerfile` |
| Docker Compose: PostgreSQL com pgvector | ✅ | `docker-compose.yaml` |
| Swagger UI com Bearer Token em dev | ✅ | `/swagger` |
| OpenAPI JSON | ✅ | `/openapi/v1.json` |
| Migrations EF Core | ✅ | `src/Persistence/Migrations/` |

---

## 5. Endpoints da API — Visão Consolidada

### 5.1. Endpoints Implementados

| Método | Rota | Descrição |
|--------|------|-----------|
| `GET` | `/` | Health check |
| `POST` | `/webhook/{channel}/{token}` | Entrada de mensagens por canal |
| `POST` | `/knowledge/ingestion` | Upload de arquivo para indexação (multipart) |
| `POST` | `/notifications` | Criar regra de notificação |
| `GET` | `/notifications` | Listar regras de notificação |
| `GET` | `/notifications/{id}` | Obter regra por ID |
| `PUT` | `/notifications/{id}` | Atualizar regra |
| `DELETE` | `/notifications/{id}` | Deletar regra |
| `POST` | `/notifications/{id}/trigger` | Disparo manual |

### 5.2. Endpoints Pendentes

| Método | Rota | Feature | Prioridade |
|--------|------|---------|------------|
| `POST` | `/channels` | Channels CRUD | 🔴 Alta |
| `GET` | `/channels` | Channels CRUD | 🔴 Alta |
| `GET` | `/channels/{id}` | Channels CRUD | 🔴 Alta |
| `PUT` | `/channels/{id}` | Channels CRUD | 🔴 Alta |
| `DELETE` | `/channels/{id}` | Channels CRUD | 🔴 Alta |
| `POST` | `/agents` | Agents CRUD | 🔴 Alta |
| `GET` | `/agents` | Agents CRUD | 🔴 Alta |
| `GET` | `/agents/{id}` | Agents CRUD | 🔴 Alta |
| `PUT` | `/agents/{id}` | Agents CRUD | 🔴 Alta |
| `DELETE` | `/agents/{id}` | Agents CRUD | 🔴 Alta |
| `POST` | `/people` | People CRUD | 🔴 Alta |
| `GET` | `/people` | People CRUD | 🔴 Alta |
| `GET` | `/people/{id}` | People CRUD | 🔴 Alta |
| `PUT` | `/people/{id}` | People CRUD | 🔴 Alta |
| `DELETE` | `/people/{id}` | People CRUD | 🔴 Alta |
| `POST` | `/people/{personId}/contacts` | Contacts CRUD | 🔴 Alta |
| `GET` | `/people/{personId}/contacts` | Contacts CRUD | 🔴 Alta |
| `GET` | `/people/{personId}/contacts/{id}` | Contacts CRUD | 🔴 Alta |
| `PUT` | `/people/{personId}/contacts/{id}` | Contacts CRUD | 🔴 Alta |
| `DELETE` | `/people/{personId}/contacts/{id}` | Contacts CRUD | 🔴 Alta |
| `GET` | `/knowledge` | Knowledge lifecycle | 🔴 Alta |
| `GET` | `/knowledge/search` | Knowledge lifecycle | 🔴 Alta |
| `DELETE` | `/knowledge/source/{sourceName}` | Knowledge lifecycle | 🔴 Alta |
| `DELETE` | `/knowledge/context/{contextName}` | Knowledge lifecycle | 🔴 Alta |
| `DELETE` | `/knowledge/{id}` | Knowledge lifecycle | 🔴 Alta |
| `GET` | `/conversations` | Conversations read | 🟡 Média |
| `GET` | `/conversations/{id}` | Conversations read | 🟡 Média |
| `GET` | `/notifications/{ruleId}/logs` | Notification Logs | 🟡 Média |
| `GET` | `/notifications/logs` | Notification Logs | 🟡 Média |
| `GET` | `/people/{personId}/notification-logs` | Notification Logs | 🟡 Média |
| `GET` | `/ai-usage` | AI Usage read | 🟡 Média |
| `GET` | `/ai-usage/summary` | AI Usage read | 🟡 Média |

---

## 6. Features Pendentes (Não relacionadas a endpoints)

| Feature | Prioridade | Detalhes |
|---------|------------|----------|
| Suporte a imagem no Knowledge (RAG) — descrição via Vision model | 🟠 Média-Alta | Detectar imagem no `DocumentExtractor`, gerar descrição, indexar chunk |
| Captura de tokens da LLM nos executores do pipeline | 🟠 Média-Alta | Integrar `IAIUsageHistoryRepository` em `AIAgentExecutor`, `KnowledgeBaseIngestor`, `SpeechToTextExecutor`, `TextToSpeechExecutor` |
| Rate Limit de mensagens por usuário/canal | 🟠 Média-Alta | Estratégia de armazenamento, config por workspace, validação no `WebhookMessageProcessor` |
| Configurar Tools/MCPs por agente | 🟠 Média-Alta | Adicionar `Tools[]` e `Mcps[]` no `AgentOptions`, filtrar no `AIAgentToolsFactory` |
| Adversarial Detection (LLM Guardrails) | 🟢 Baixa | Detectar prompt injection/jailbreak, flag no `AgentOptions`, sanitizar antes de enviar |
| System Prompt Separation (Instruction Tuning) | 🟢 Baixa | Delimitar user message vs system message, evitar contaminação de instruções |

---

## 7. Regras de Implementação para Agentes de Código

### 7.1. Padrões Obrigatórios

1. **Isolamento por workspace**: toda query ao banco DEVE filtrar por `WorkspaceId`. Nunca retornar dados de outro tenant.
2. **Autenticação**: todos os controllers DEVEM usar `[Authorize]` e obter o workspace via `User.GetWorkspaceId()`.
3. **DTOs**: usar records para Input/Output DTOs. Output DTOs devem ter método `FromEntity` estático para mapeamento.
4. **Service layer**: controllers delegam para services (interface + implementação). Registrar no DI em `Program.cs`.
5. **EF Core**: usar `AppDbContext` via injeção. Mappings ficam em `src/Persistence/Mappings/`. Novos DbSets no `AppDbContext`.
6. **Snake case**: banco usa snake_case via `.UseSnakeCaseNamingConvention()`.
7. **JSON columns**: campos complexos (metadata, lists) usar `jsonb` via `.HasColumnType("jsonb")`.
8. **Enums como string**: `ConfigureConventions` já converte enums para string.

### 7.2. Estrutura de Diretórios

```
src/
├── Entities/           # Entidades do domínio (classes POCO)
├── Features/           # Features organizadas por domínio
│   ├── AI/             # Serviços de IA (Agent, Audio, Embedding, Usage)
│   ├── Channels/       # Canais (Webhook, WhatsApp, Telegram)
│   ├── Knowledgebase/  # Base de conhecimento (Ingestão, Chunkers)
│   ├── Notifications/  # Notificações (Rules, Logs, Dispatcher)
│   ├── Workflows/      # Pipeline de processamento
│   └── Workspaces/     # Contexto e auth do workspace
├── Persistence/        # EF Core (DbContext, Mappings, Migrations)
├── Queue/              # Fila in-memory
└── Utils/              # Utilitários
```

### 7.3. Convenções de Nomenclatura

| Elemento | Convenção | Exemplo |
|----------|-----------|---------|
| Controller | `{Resource}Controller` | `ChannelsController` |
| Service Interface | `I{Resource}Service` | `IChannelService` |
| Service Impl | `{Resource}Service` | `ChannelService` |
| DTO Input | `{Resource}Input` | `ChannelInput` |
| DTO Output | `{Resource}Output` | `ChannelOutput` |
| Entity | Nome singular | `Channel` |
| DbSet | Nome plural | `Channels` |
| Rota | Plural, kebab-case | `/channels`, `/people/{personId}/contacts` |

### 7.4. Exemplo de Referência

O `NotificationRulesController` + `NotificationRuleService` + `NotificationRuleDTOs` é o padrão de referência para implementar CRUDs. Seguir exatamente o mesmo padrão de:
- Controller com `[ApiController]`, `[Route]`, `[Authorize]`
- `WorkspaceId` via `User.GetWorkspaceId()`
- Service interface com operações CRUD
- DTOs com `FromEntity` para mapeamento
- Registro no `Program.cs` via `AddScoped`

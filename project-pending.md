# Análise do Estado Atual do Projeto

## Resumo Executivo

O projeto possui uma base sólida com entidades bem modeladas e funcionalidades de domínio implementadas. No entanto, **a maioria dos recursos não possui endpoints de API completos**, tornando impossível gerenciar seu ciclo de vida via API sem acesso direto ao banco de dados. Abaixo está o mapeamento detalhado de cada recurso e seus gaps.

---

## Mapeamento de Recursos × Operações de API

### Legenda

| Símbolo | Significado |
|---------|-------------|
| ✅ | Implementado |
| ❌ | Ausente (endpoint/operação não existe) |
| ⚠️ | Parcialmente implementado |

---

### 1. Workspace

**Entidade:** `src/Entities/Workspace.cs`

> ℹ️ Workspaces equivalem a contas/tenants. **Não devem ter endpoints de API** — são criados e gerenciados internamente. Sem tarefas pendentes neste recurso.

---

### 2. Channels (Canais)

**Entidade:** `src/Entities/Channel.cs`
**Configuração:** inserida manualmente no banco (sem controller)

| Operação | Status | Observação |
|----------|--------|------------|
| Criar canal | ❌ | Não existe endpoint |
| Listar canais | ❌ | Não existe endpoint |
| Obter canal por ID | ❌ | Não existe endpoint |
| Atualizar canal | ❌ | Não existe endpoint (credenciais Evolution/Telegram) |
| Deletar canal | ❌ | Não existe endpoint |

> **Impacto crítico:** as credenciais dos canais (Evolution API, Telegram) só podem ser configuradas via banco.

---

### 3. AgentDefinition (Agentes/Workflows)

**Entidade:** `src/Entities/AgentDefinition.cs`
**Configuração:** inserida manualmente no banco (sem controller)

| Operação | Status | Observação |
|----------|--------|------------|
| Criar agente/workflow | ❌ | Não existe endpoint |
| Listar agentes | ❌ | Não existe endpoint |
| Obter agente por ID | ❌ | Não existe endpoint |
| Atualizar agente | ❌ | Não existe endpoint (instructions, webhook token, canal) |
| Deletar agente | ❌ | Não existe endpoint |

> **Impacto crítico:** o `webhook_token` que roteia mensagens dos canais só pode ser criado/alterado via banco.

---

### 4. People (Pessoas)

**Entidade:** `src/Entities/Person.cs`
**Configuração:** criada automaticamente pelo WebhookMessageProcessor, mas sem controller

| Operação | Status | Observação |
|----------|--------|------------|
| Criar pessoa | ❌ | Criação automática no webhook, mas sem endpoint manual |
| Listar pessoas | ❌ | Não existe endpoint (necessário para `target_type: Specific` e `Dynamic`) |
| Obter pessoa por ID | ❌ | Não existe endpoint |
| Atualizar pessoa | ❌ | Não existe endpoint (nome, metadados usados em filtros de notificação) |
| Deletar pessoa | ❌ | Não existe endpoint |

> **Impacto:** sem listar pessoas, não é possível configurar corretamente as regras de notificação com `target_type = Specific` nem verificar os metadados para `Dynamic`.

---

### 5. Contacts (Contatos)

**Entidade:** `src/Entities/Contact.cs`
**Configuração:** criada automaticamente pelo webhook, mas sem controller

| Operação | Status | Observação |
|----------|--------|------------|
| Criar contato | ❌ | Criação automática no webhook, sem endpoint manual |
| Listar contatos por pessoa | ❌ | Não existe endpoint |
| Obter contato por ID | ❌ | Não existe endpoint |
| Atualizar contato | ❌ | Não existe endpoint |
| Deletar contato | ❌ | Não existe endpoint |

---

### 6. Knowledge (Base de Conhecimento)

**Entidade:** `src/Entities/Knowledge.cs`
**Controller:** `src/Features/Knowledgebase/KnowledgeController.cs`

| Operação | Status | Observação |
|----------|--------|------------|
| Ingerir documento | ✅ | `POST /knowledge/ingestion` |
| Listar documentos ingeridos | ❌ | Não existe endpoint (impossível auditar o que foi indexado) |
| Obter chunk por ID | ❌ | Não existe endpoint |
| Buscar documentos (pesquisa semântica) | ❌ | Não existe endpoint público de busca |
| Deletar documento/chunks por source | ❌ | Não existe endpoint (impossível remover um documento errado) |
| Deletar todos os chunks de um contexto | ❌ | Não existe endpoint |

> **Exemplo exato do problema mencionado:** é possível fazer a ingestão de um documento, mas não é possível consultá-lo, pesquisá-lo ou removê-lo via API.

---

### 7. Conversations (Conversas)

**Entidade:** `src/Entities/Conversation.cs`

| Operação | Status | Observação |
|----------|--------|------------|
| Listar conversas | ❌ | Não existe endpoint |
| Obter conversa por ID (com mensagens) | ❌ | Não existe endpoint |
| Deletar/expirar conversa | 🚫 | Não deve ter endpoint (gerenciado internamente por expiração) |

---

### 8. Notification Rules (Regras de Notificação)

**Controller:** `src/Features/Notifications/NotificationRulesController.cs`

| Operação | Status | Observação |
|----------|--------|------------|
| Criar regra | ✅ | `POST /notifications` |
| Listar regras | ✅ | `GET /notifications` |
| Obter regra por ID | ✅ | `GET /notifications/{id}` |
| Atualizar regra | ✅ | `PUT /notifications/{id}` |
| Deletar regra | ✅ | `DELETE /notifications/{id}` |
| Disparar regra manualmente | ✅ | `POST /notifications/{id}/trigger` |

> ✅ **Único recurso com CRUD completo implementado.**

---

### 9. Notification Logs (Histórico de Notificações)

**Entidade:** `src/Entities/NotificationLog.cs`

| Operação | Status | Observação |
|----------|--------|------------|
| Listar logs por regra | ❌ | Não existe endpoint |
| Listar logs por batch | ❌ | Não existe endpoint |
| Listar logs por pessoa | ❌ | Não existe endpoint |

---

### 10. AI Usage History (Histórico de Uso de IA)

**Entidade:** `src/Entities/AIUsageHistory.cs`
**Repositório:** `src/Features/AI/Usage/AIUsageHistoryRepository.cs`

| Operação | Status | Observação |
|----------|--------|------------|
| Registrar uso (interno) | 🚫 | Não deve ter endpoint — chamado internamente pelo pipeline |
| Listar histórico de uso | ❌ | Não existe endpoint (impossível auditar tokens consumidos) |
| Consolidado por período | ❌ | Não existe endpoint |

---

## Resumo Geral por Recurso

| Recurso | Create | Read | Update | Delete | Status Geral |
|---------|--------|------|--------|--------|--------------|
| Workspace | 🚫 | 🚫 | 🚫 | 🚫 | 🚫 Sem API — gerenciado internamente |
| Channel | ❌ | ❌ | ❌ | ❌ | ❌ Nenhum endpoint |
| AgentDefinition | ❌ | ❌ | ❌ | ❌ | ❌ Nenhum endpoint |
| Person | ❌ | ❌ | ❌ | ❌ | ❌ Nenhum endpoint |
| Contact | ❌ | ❌ | ❌ | ❌ | ❌ Nenhum endpoint |
| Knowledge | ✅ | ❌ | — | ❌ | ⚠️ Apenas ingestão |
| Conversation | — | ❌ | — | 🚫 | ⚠️ Apenas leitura pendente |
| NotificationRule | ✅ | ✅ | ✅ | ✅ | ✅ CRUD completo |
| NotificationLog | — | ❌ | — | — | ❌ Nenhum endpoint |
| AIUsageHistory | 🚫 | ❌ | — | — | ⚠️ Leitura pendente |

🚫 = Sem endpoint por decisão de design

---

## Lista de Tarefas Pendentes

### 🔴 Alta Prioridade — Recursos sem nenhum endpoint de gerenciamento

#### [T1] Implementar CRUD de Channels (Canais)
- [ ] `POST /channels` — criar canal (com credenciais Evolution/Telegram)
- [ ] `GET /channels` — listar canais do workspace
- [ ] `GET /channels/{id}` — obter canal por ID
- [ ] `PUT /channels/{id}` — atualizar canal (credenciais)
- [ ] `DELETE /channels/{id}` — deletar canal

#### [T2] Implementar CRUD de AgentDefinitions (Agentes/Workflows)
- [ ] `POST /agents` — criar agente/workflow (com webhook token, associação de canais, instruções)
- [ ] `GET /agents` — listar agentes do workspace
- [ ] `GET /agents/{id}` — obter agente por ID
- [ ] `PUT /agents/{id}` — atualizar agente (instruções, canais, opções)
- [ ] `DELETE /agents/{id}` — deletar agente

#### [T3] Implementar CRUD de People (Pessoas)
- [ ] `POST /people` — criar pessoa manualmente
- [ ] `GET /people` — listar pessoas do workspace (com paginação)
- [ ] `GET /people/{id}` — obter pessoa por ID (com contatos)
- [ ] `PUT /people/{id}` — atualizar pessoa (nome, metadados — necessário para filtros de notificação)
- [ ] `DELETE /people/{id}` — deletar pessoa

#### [T4] Implementar CRUD de Contacts (Contatos)
- [ ] `POST /people/{personId}/contacts` — adicionar contato a uma pessoa
- [ ] `GET /people/{personId}/contacts` — listar contatos de uma pessoa
- [ ] `GET /people/{personId}/contacts/{id}` — obter contato por ID
- [ ] `PUT /people/{personId}/contacts/{id}` — atualizar contato (canal, identifier)
- [ ] `DELETE /people/{personId}/contacts/{id}` — deletar contato

---

### 🔴 Alta Prioridade — Knowledge com ciclo de vida incompleto

#### [T5] Completar o ciclo de vida da Knowledge Base
- [ ] `GET /knowledge` — listar sources/documentos ingeridos do workspace (agrupados por source e context)
- [ ] `GET /knowledge/search?q=...&context=...` — endpoint de busca semântica (para debug e validação)
- [ ] `DELETE /knowledge/source/{sourceName}` — remover todos os chunks de um documento por source
- [ ] `DELETE /knowledge/context/{contextName}` — remover todos os chunks de um contexto
- [ ] `DELETE /knowledge/{id}` — remover chunk específico por ID

---

### 🟡 Média Prioridade — Recursos auditáveis sem endpoints de leitura

#### [T6] Endpoints de leitura para Conversations
- [ ] `GET /conversations` — listar conversas do workspace (com filtros: pessoa, canal, período)
- [ ] `GET /conversations/{id}` — obter conversa por ID com histórico de mensagens

#### [T7] Endpoints de leitura para Notification Logs
- [ ] `GET /notifications/{ruleId}/logs` — listar histórico de execuções de uma regra (com paginação)
- [ ] `GET /notifications/logs?batchId={guid}` — listar logs de um batch de execução específico
- [ ] `GET /people/{personId}/notification-logs` — listar notificações enviadas para uma pessoa

#### [T8] Endpoints de leitura para AI Usage History
- [ ] `GET /ai-usage` — listar histórico de uso de IA do workspace (com filtros por período e serviço)
- [ ] `GET /ai-usage/summary` — consolidado de tokens/custo agrupado por período (diário, mensal)

---

### 🟠 Média-Alta Prioridade — Itens do todo.md existente

#### [T9] Implementar suporte a imagem no Knowledge (RAG)
- [ ] Detectar arquivos de imagem no `DocumentExtractor`
- [ ] Gerar descrição textual via Vision model (GPT-4o ou similar)
- [ ] Indexar a descrição gerada como chunk na base de conhecimento

#### [T10] Registrar uso de tokens da LLM
- [ ] Capturar tokens de Chat (input/output) no `AIAgentExecutor`
- [ ] Capturar tokens de Embedding no `KnowledgeBaseIngestor`
- [ ] Capturar tokens de STT no `SpeechToTextExecutor`
- [ ] Capturar tokens de TTS no `TextToSpeechExecutor`
- [ ] Salvar registros via `IAIUsageHistoryRepository`

#### [T11] Implementar Rate Limit de mensagens por usuário/canal
- [ ] Definir estratégia de armazenamento: em memória, Redis ou banco
- [ ] Configurar limites por workspace no metadata do workspace
- [ ] Integrar validação no `WebhookMessageProcessor` antes de enfileirar a mensagem

#### [T12] Configurar no AgentOptions quais grupos de Tools/MCPs utilizar
- [ ] Adicionar `Tools: string[]` e `Mcps: string[]` no `AgentOptions`
- [ ] Filtrar tools e MCPs carregados pelo `AIAgentToolsFactory` conforme a configuração
- [ ] Documentar no README

---

### 🟢 Baixa Prioridade — Melhorias de segurança e robustez

#### [T13] Implementar Adversarial Detection (LLM Guardrails)
- [ ] Detectar tentativas de prompt injection / jailbreak nas mensagens de entrada
- [ ] Configurar no `AgentOptions` se o guardrail está habilitado
- [ ] Bloquear ou sanitizar mensagens suspeitas antes de enviar ao modelo

#### [T14] Implementar System Prompt Separation (Instruction Tuning)
- [ ] Delimitar claramente a mensagem do usuário e a system message no prompt
- [ ] Garantir que o conteúdo do usuário não contamine as instruções do sistema

---

> **Conclusão:** dos 9 recursos com endpoints de API, apenas **NotificationRule** possui CRUD completo. **Workspace** não tem e não deve ter endpoints (gerenciado internamente). Os demais precisam de endpoints básicos de leitura, e recursos como **Channel** e **AgentDefinition** precisam de CRUD completo para que a API seja operável sem acesso direto ao banco de dados.

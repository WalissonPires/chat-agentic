# Chat Agentic

API REST para atendimento conversacional omnichannel com agente de IA. O sistema recebe mensagens dos usuários pelos canais que você integrar (por exemplo WhatsApp ou Telegram), processa cada interação em um fluxo orientado a workflow e devolve a resposta pelo mesmo canal, mantendo continuidade da conversa e acesso a conhecimento contextual quando necessário.

O desenho de dados gira em torno de **workspaces**: cada workspace é um ambiente fechado, pensado para **multi-tenant**. Assim, você pode operar vários clientes, produtos ou departamentos na mesma API sem cruzar histórico, identidades nem documentação entre eles.

Dentro de um workspace, **pessoas** representam quem conversa com o agente. Uma mesma pessoa pode aparecer em mais de um canal; por isso existem **contatos** ligados a ela e cada contato associa o canal (WhatsApp, Telegram, entre outros) ao identificador concreto naquele meio, como número de telefone ou nome de usuário. Isso permite unificar a conversa em torno de um perfil, mesmo quando o usuário alterna ou duplica canais.

A **base de conhecimento** também vive dentro do workspace e pode ser **segmentada por contexto** (por exemplo manuais para usuário final, guias técnicos para desenvolvimento ou material de serviços internos). Na hora de responder, o RAG pode priorizar o conjunto certo de documentos, mantendo respostas alinhadas ao público e ao assunto.

O foco da solução é uma base única de orquestração: um ponto de entrada por webhook, processamento assíncrono com fila, e extensibilidade por ferramentas (tools), habilidades declarativas (skills) e recuperação de informação sobre essa base segmentada.

---

## Visão técnica

### Arquitetura em alto nível

1. O canal de mensageria envia eventos para a API via **webhook** autenticado por token.
2. A mensagem é enfileirada e consumida por um serviço em background.
3. Um **workflow** encadeia etapas: carregar contexto, persistir conversa, transcrever mídia quando aplicável, executar o **agente de IA** (com tools, skills e busca semântica), opcionalmente síntese de voz, e por fim **responder no canal**.

Essa separação entre recepção HTTP e processamento permite escalar o trabalho pesado e manter o webhook rápido e resiliente.

### Multi-tenant e domínio de dados

Todo o fluxo respeita o **workspace** como fronteira: pessoas, contatos por canal e fragmentos de conhecimento (com seu **contexto** lógico) pertencem a um único workspace. Webhooks e ingestão de documentos devem ser configurados de forma que mensagens e arquivos caiam no tenant correto, preservando isolamento entre ambientes.

As credenciais e parâmetros do **provedor de IA** (OpenAI ou compatível), da **Evolution API** (WhatsApp) e da **Telegram Bot API** ficam no campo JSON **`metadata`** de cada workspace no banco de dados.

### Stack

| Área | Tecnologia |
|------|------------|
| Runtime e API | C#, **.NET 10**, ASP.NET Core |
| Agentes e workflows | **Microsoft Agent Framework** (pacotes `Microsoft.Agents.AI.*` — OpenAI e Workflows) |
| Persistência | **PostgreSQL** com Entity Framework Core; extensão **pgvector** para embeddings e busca vetorial |
| Integração de canais | Webhooks genéricos por tipo de canal; envio/recebimento via Evolution API (WhatsApp) e Telegram Bot API |

### Recursos do agente

- **Workflows**: pipeline declarativo de executores (contexto, persistência, STT, agente, TTS, resposta).
- **Tools**: integração com **Model Context Protocol (MCP)** para expor ferramentas externas ao modelo.
- **Skills**: instruções em Markdown carregadas do disco, alinhadas ao padrão de skills do ecossistema de agentes.
- **RAG**: ingestão de documentos, chunking, embeddings e busca textual/vetorial sobre a base de conhecimento do workspace, com fatias por **contexto** quando aplicável.

### Endpoints principais

- `GET /` — verificação de disponibilidade.
- `POST /webhook/{channel}/{token}` — entrada de mensagens por canal (o `channel` identifica o tipo; o `token` autentica a instância).
- `POST /knowledge/ingestion` — upload de arquivo para indexação na base de conhecimento (multipart).

Documentação OpenAPI é exposta em ambiente de desenvolvimento.

---

## Skills e tools do agente

Crie uma pasta `.agent` na raiz do projeto com a seguinte estrutura:

```
.agent/
│
├── skills/
│   ├── my-skill-1/
│   │   └── SKILL.md
│   └── my-skill-2/
│       └── SKILL.md
│
└── tools/
    ├── tool-1/
    │   └── TOOL.json
    └── tool-2/
        ├── tool-files
        └── TOOL.json
```

Skills e tools são carregadas automaticamente pela aplicação.

### Exemplo de `SKILL.md`

```markdown
---
name: midesp-total-expense-monthly
description: Returns the total expenses for a month (one or all participants).
compatibility: Requires midesp tool
metadata:
  author: midesp
  version: "1.0.0"
  keywords: [expenses, total, monthly, current, spending, month, participant]

---

Objective: To obtain the total amount of expenses for a specific month.

When to use:
- Questions about total monthly expenses
- Monthly expense queries (user or general)

When NOT to use:
- Queries by category, tags, payment methods
- Queries for periods other than one month
- Expense listing

Steps:

1. If the query is only for the user:
  - Call `account_me_get`
  - Extract `participant_id` from `config` (config is a JSON string that is an object with: { "participant_id": id })

2. Call `dashboard_get_cards` with:
  - `year`
  - `month`
  - `card`: `card_participants`
  - `participant_id` (optional)

3. Result:
  - If there is a `participant_id`: return the participant's total
  - Otherwise: sum all participants and return the total

Output:
  - Return only the total value (Value format R$ 0.00)
```

### Exemplo de `TOOL.json` (MCP SSE)

Na pasta da tool crie o arquivo `TOOL.json` com as configurações do servidor MCP http:

```json
{
  "Enabled": true,
  "Type": "SSE",
  "Endpoint": "http://localhost:5007",
  "Headers": {
    "ApiKey": "TOKEN"
  }
}
```

### Exemplo de `TOOL.json` (MCP STDIO)

Na pasta da tool adicione os arquivos do servidor mcp e crie o `TOOL.json` com instruções para iniciar o processo:

```json
{
  "Enabled": true,
  "Type": "STDIO",
  "Command": "node",
  "WorkDir": "",
  "Args": [
    "dist/index.js"
  ],
  "Envs": {
    "MIDESP_API_BASE_URL": "https://api.midesp.com.br"
  }
}
```

---

## Configuração para desenvolvimento

### Pré-requisitos

- [.NET SDK 10](https://dotnet.microsoft.com/download)
- Docker (recomendado) para subir o PostgreSQL com pgvector

### Executar dependências

Na raiz do repositório execute:

```bash
docker compose up -d
```

### Configuração da aplicação

1. Copie o `appsettings.json` com o nome de `appsettings.Development.json`.
2. Defina `ConnectionString` apontando para o Postgres (host `localhost`, porta alinhada ao Compose, por exemplo `15432`).

### Configuração por workspace (`metadata`)

Para cada `workspaces`, preencha a coluna JSON **`metadata`** com os blocos que o tenant usar.

Exemplo ilustrativo (ajuste valores e omita blocos de canais que não usar):

```json
{
    "EvolutionApi": {
        "ServerUrl": "https://evoapi-server.com.br",
        "ApiKey": "TOKEN",
        "Instance": "INSTANCE_NAME"
    },
    "Telegram": {
        "BotToken": "TOKEN",
        "BaseUrl": "https://api.telegram.org",
        "FileBaseUrl": "https://api.telegram.org/file"
    },
    "AIProvider": {
        "ApiKey": "TOKEN",
        "Endpoint": "https://api.openai.com/v1",
        "ChatModel": "gpt-5-nano",
        "ImageModel": "dall-e-3",
        "EmbedModel": "text-embedding-3-small",
        "TranscriptionModel": "whisper-1",
        "TtsModel": "tts-1",
        "TtsVoice": "alloy"
    }
}
```

- **Webhook**: o workspace precisa de `webhook_token` coerente com a URL `/webhook/{channel}/{token}`.
- **Ingestão de conhecimento**: use `integration_token` no formulário multipart; o servidor resolve o workspace e aplica o mesmo `metadata` para gerar embeddings.

### Executar a API

```bash
dotnet run
```

### Crie endpoint para receber eventos de webhook

Utilize o ngrok para criar um endpoint e registre o endpoint nos canais utilizados.

```bash
ngrok http 5010
```

Em desenvolvimento, a documentação OpenAPI fica disponível em http://localhost:5010/openapi/v1.json.

### Configuração de webhook no Telegram

Com o bot criado no Telegram, configure o webhook para a URL da API:

```bash
curl -X POST "https://api.telegram.org/bot<SEU_BOT_TOKEN>/setWebhook" \
  -d "url=https://SEU_DOMINIO/webhook/telegram/<TOKEN_DO_WORKSPACE>"
```

---

## Executar a API usando docker

### Construir a imagem localmente

Na raiz do projeto execute:

```bash
docker build -t chat-agentic:local .
```

### Executar com `docker run` e variáveis de ambiente

Exemplo em um único comando (Linux, macOS ou Git Bash no Windows), com Postgres acessível na máquina host na porta publicada `15432` (por exemplo após `docker compose up`):

```bash
docker run -d --name chat-agentic \
  -p 8080:8080 \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionString="Host=host.docker.internal;Port=15432;Database=chatagentic;Username=postgres;Password=SUA_SENHA_POSTGRES" \
  seuusuario/chat-agentic:latest
```

- Se o Postgres rodar **em outro container** na mesma rede Docker, use o nome do serviço como host (por exemplo `Host=postgres;Port=5432;...`) e conecte os containers com `--network` adequado em vez de `host.docker.internal`.

Para inspecionar logs:

```bash
docker logs -f chat-agentic
```
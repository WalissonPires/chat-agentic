
# Usando tool MCP e skills

Crie uma pasta `.agent` na raiz do projeto com a seguinte estrutura:

```
.agent
  skills
    my-skill-1
    my-skill-2
      SKILL.md
  tools
    tool-1
    tool-2
      tool-files
      TOOL.json
```

As skills e tools serão carregadas automaticamente.

Exemplo de SKILL.md

```markdown
---
name: my-skill-2
description: Describe what the tool does.
compatibility: Requires midesp tool
metadata:
  author: midesp
  version: "1.0.0"
  keywords: [despesas, total, mensal, atual, gastos, mês, participante]
---

Objetivo:
Obter o valor total de despesas de um mês específico.

Quando usar:
- Perguntas sobre total de despesas mensais
- Consultas de gastos por mês (usuário ou geral)

Quando NÃO usar:
- Consultas por categoria, tags, métodos de pagamento
- Consultas por período diferente de um mês
- Listagem de despesas

Passos:
1. Se a consulta for apenas do usuário:
   - Chamar `account_me_get`
   - Extrair `participant_id` de `config` (config é uma string JSON que é um objeto com: { "participant_id": id })

2. Chamar `dashboard_get_cards` com:
   - `year`
   - `month`
   - `card`: `card_participants`
   - `participant_id` (opcional)

3. Resultado:
   - Se houver `participant_id`: retornar o total do participante
   - Caso contrário: somar todos os participantes e retornar total

Saída:
- Retornar apenas o valor total (Valor no formato R$ 0,00)
```

Exemplo de TOOL.json:

Para MCP SSE:
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

Para MCP STDIO:
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
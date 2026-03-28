# MiDesp - Documentação Completa do Usuário

## 1. Introdução

O **MiDesp** é uma aplicação web desenvolvida para auxiliar no controle e organização de despesas financeiras.

A plataforma permite que usuários registrem gastos, organizem despesas por categorias e tags, analisem relatórios financeiros e integrem suas contas bancárias para automatizar o controle financeiro.

O principal objetivo do sistema é proporcionar uma visão clara das finanças pessoais ou compartilhadas, ajudando usuários a entender melhor seus hábitos de consumo.

---

# 2. Principais Conceitos do Sistema

Antes de utilizar o MiDesp, é importante entender alguns conceitos fundamentais da aplicação.

## Despesa

Uma **despesa** representa qualquer gasto realizado pelo usuário.

Cada despesa pode conter:

* valor
* data
* categoria
* tags
* método de pagamento
* participante responsável
* observações

---

## Categoria

Categorias são utilizadas para **classificar os tipos de gastos**.

Exemplos:

* Alimentação
* Transporte
* Moradia
* Lazer
* Assinaturas
* Saúde

Isso permite visualizar relatórios de gastos por tipo.

---

## Tags

As **tags** permitem adicionar um segundo nível de organização.

Exemplos:

* Mercado
* Restaurante
* Uber
* Streaming
* Viagem

Tags ajudam a detalhar ainda mais os gastos.

---

## Participantes

Participantes representam **pessoas envolvidas nas despesas**.

Pode ser usado para:

* controle de gastos em casal
* controle de despesas familiares
* divisão de despesas

Exemplos:

* João
* Maria
* Família
* Empresa

---

## Método de pagamento

Define **como a despesa foi paga**.

Exemplos:

* Cartão de crédito
* Cartão de débito
* Pix
* Dinheiro
* Transferência

---

# 3. Acessando o sistema

Acesse o site oficial:

https://midesp.com.br

Depois:

1. Clique em **Entrar**
2. Faça login com sua conta
3. Após login você será direcionado ao **Dashboard**

---

# 4. Dashboard

O **Dashboard** apresenta uma visão geral das despesas.

Informações disponíveis:

* total gasto no mês
* gráfico de gastos por categoria
* gráfico de gastos por tag
* gráfico de gastos por método de pagamento
* gráfico de gastos por participante
* evolução de gastos ao longo do tempo
* gráfico de consumo do salario baseado nos limites definidos nas tags e categorias

O dashboard permite identificar rapidamente:

* onde o dinheiro está sendo gasto
* categorias e tags que consomem mais recursos
* tendências de gastos

---

# 5. Cadastro de Categorias

As categorias ajudam a organizar os tipos de despesas.

## Como criar uma categoria

1. Acesse o menu **Cadastros**
2. Clique em **Categorias**
3. Clique em **Nova Categoria**
4. Preencha as informações:

Campos:

* Nome da categoria
* Cor da categoria (opcional)

5. Clique em **Salvar**

### Exemplo

Nome: Alimentação
Cor: Verde

---

## Editar categoria

1. Vá em **Cadastros → Categorias**
2. Clique na categoria desejada
3. Faça as alterações
4. Clique em **Salvar**

## Distribuição do salario

É possível definir o quanto do salario podera ser gasto em cada categoria.
Na dashbord será exibido um badge nos cards de despsas com o percentual atualmente atingido baseado nas despesas do mês

---

# 6. Cadastro de Tags

Tags permitem organizar despesas com mais detalhes.

## Criar uma tag

1. Acesse **Cadastros**
2. Clique em **Tags**
3. Clique em **Nova Tag**
4. Preencha:

* Nome da tag
* Cor (opcional)

5. Clique em **Salvar**

### Exemplos de tags

* Mercado
* Restaurante
* Viagem
* Delivery
* Streaming

## Distribuição do salario

É possível definir o quanto do salario podera ser gasto em cada tag.
Na dashbord será exibido um badge nos cards de despsas com o percentual atualmente atingido baseado nas despesas do mês

---

# 7. Cadastro de Participantes

Participantes representam as pessoas associadas às despesas.

## Criar participante

1. Acesse **Cadastros**
2. Clique em **Participantes**
3. Clique em **Novo Participante**
4. Preencha:

Campos:

* Nome
* Cor (opcional)

5. Clique em **Salvar**

### Exemplo

Nome: João

Isso permite filtrar gastos por pessoa.

---

# 8. Cadastro de Métodos de Pagamento

Métodos de pagamento indicam como a despesa foi realizada.

## Criar método de pagamento

1. Acesse **Cadastros**
2. Clique em **Métodos de Pagamento**
3. Clique em **Novo**
4. Preencha:

Campos:

* Nome do método
* Tipo (opcional)

Exemplos:

* Cartão Nubank
* Pix
* Dinheiro
* Cartão Débito

5. Clique em **Salvar**

---

# 9. Cadastro de Despesas

Registrar despesas é a funcionalidade principal da aplicação.

## Criar nova despesa

1. Acesse **Despesas**
2. Clique em **Nova Despesa**

Preencha:

Valor — valor gasto
Data — data da despesa
Categoria — tipo de despesa
Método de pagamento — forma de pagamento
Participante — pessoa associada
Tags — organização adicional
Descrição — observações opcionais

3. Clique em **Salvar**

---

# 10. Integração com banco

O MiDesp permite integração com bancos para importar automaticamente transações.

Isso reduz a necessidade de cadastro manual.

## Como configurar integração

1. Acesse **Configurações**
2. Clique em **Integrações**
3. Selecione **Banco**
4. Escolha o banco desejado

Atualmente suportado:

* Nubank

5. Insira os dados solicitados
6. Autorize a conexão
7. Clique em **Salvar**

Após configuração, o sistema poderá importar transações automaticamente.

---

# 11. Importação de arquivos OFX

Caso o banco não esteja integrado diretamente, é possível importar arquivos OFX.

Arquivos **OFX** são extratos financeiros exportados pelos bancos.

---

## Como exportar OFX do banco

1. Acesse o site do seu banco
2. Vá na área de **extrato**
3. Procure opção **exportar extrato**
4. Escolha formato **OFX**
5. Baixe o arquivo

---

## Como importar OFX no MiDesp

1. Acesse **Despesas**
2. Clique em **Importar OFX**
3. Selecione o arquivo OFX no seu computador
4. Clique em **Importar**

O sistema irá:

* ler o arquivo
* extrair transações
* sugerir categorias

Você poderá revisar antes de salvar.

---

# 12. Organização das despesas

Para manter os dados organizados recomenda-se:

* sempre usar categorias
* utilizar tags quando necessário
* registrar participantes
* manter métodos de pagamento atualizados

Isso melhora os relatórios e análises.

---

# 13. Análise de despesas

Utilize o **Dashboard** para analisar dados financeiros.

É possível visualizar:

* despesas por categoria
* despesas por participante
* despesas por método pagamento
* evolução mensal de gastos

Essas análises ajudam a:

* identificar gastos excessivos
* controlar orçamento
* melhorar planejamento financeiro

---

# 14. Boas práticas

Para melhor uso da aplicação:

* registre despesas regularmente
* categorize corretamente os gastos
* utilize tags para maior detalhamento
* revise dashboard semanalmente
* utilize integração bancária quando possível

---

# 15. Suporte

Caso tenha dúvidas ou problemas, utilize os canais de suporte disponíveis no site:

https://midesp.com.br

---

# 16. Conclusão

O **MiDesp** é uma ferramenta poderosa para organização financeira.

Utilizando corretamente categorias, tags, participantes e métodos de pagamento, é possível obter uma visão clara das finanças e melhorar o controle sobre os gastos.

- [ ] Adicionar suporte a imagem no knowledge (RAG)
- [ ] Instruir a LLM para gerar a resposta em JSON e dizer para ela que a resposta será convertida em audio. Portanto ele deve retorna de forma separada qualquer conteudo que não deve ser falado e deve ser enviado como texto. Ex.: Links, Código de barras e etc
  { "message": "", "contents": [{ "type": "site_url", "label": "Link para pagamento", "value": "https://sample-url.com" }, { "type": "image_url", "label": "Imagem da area do cliente", "value": "https://sample-url.com/img1" }] }
- [ ] Registrar uso de tokens da LLM
- [ ] Implementar RateLimit de mensagem por usuário/canal
- [ ] Implementar Adversarial Detection (LLM Guardrails)
- [ ] Implementar System Prompt Separation (Instruction Tuning) (Delimitar mensagem do usuário)
- [ ] Obter config AIProvider do workspace em vez do appsettings
  ### UserMessage
  ...
  ### SystemMessage
  ...
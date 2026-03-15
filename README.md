

- Webhook (webhookToken) -> Get phone, message e files -> (Audio) SpeechToText -> MessageDistillation -> Load ChatHistory -> LLM Agent (RAG/Tool) -> TextToSpeech -> SendMessage -> Save ChatHistory

Context (Midesp) -> RAG(midesp) / Tools (Midesp)
Context (IPTV) -> RAG(IPTV) / Tools (IPTV)


Workspace(id, name, meta(JSONB), webhookToken) | Context('midesp', meta(tools: [a,b,c,x,y,z]))
WorkspaceMeta(schemaVersion, providerUrl, providerKey, chatModel, embedModel, transcriptionModel, ttsModel, ttsVoice, tools)
Knowledge(workspaceId, contentId, content, embded, createdAt)

UsersMetadata(userId, key, value) | UserMeta(1, 'midesp.tools', '[x,y,z]'), UserMeta(2, 'midesp.tools', '[a,b,c]')
User(id, workspaceId)
UserChannels(id, userId, channel, identifier)
ChannelType(Whatsapp, Telegram, Email)
Sessions(id, userId, channel, identifier, expireAt, ...)
ChatHistory(id, sessionId, createdAt, role, messsageId, messageText, messageType, messageMime, messsageFile)
MessageType(text, file)
MessageRole(user, assistent, system)

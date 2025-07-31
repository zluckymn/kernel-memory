# AI Agent Instructions for SemanticKernel Project

## Project Overview
This is a .NET-based multi-agent system using Microsoft's Semantic Kernel framework to orchestrate multiple AI services for chat-based interactions. The system demonstrates advanced patterns for handling streaming responses, chat history management, and coordinated multi-agent processing.

## Key Architecture Components

### 1. Multi-Agent Orchestration
- Core orchestration happens in `MultiAgentService.cs`
- Uses three specialized AI services:
  - OpenAI Translation (gpt-4)
  - QWen3 Intent Recognition
  - Ollama Extraction (local LLama model)
- Each agent has its own `Kernel` instance for isolation

### 2. Chat History Management
- Implemented in `ChatHistoryService.cs`
- Handles history compression with intelligent summarization
- Uses OpenAI Translation service for summarization
- Token threshold-based reduction strategy (500 tokens)

### 3. API Interface
- Streaming SSE (Server-Sent Events) endpoint at `/api/QueryByStream`
- Structured request format: `QueryRequest(string Query, List<ChatMessageContent> History)`
- Headers configured for streaming: `text/event-stream`, `no-cache`, `keep-alive`

## Development Patterns

### Service Registration
```csharp
// Register AI services with unique keys for different purposes
builder.Services.AddKeyedSingleton<IChatCompletionService>("OpenAI_Translation", ...);
builder.Services.AddKeyedSingleton<IChatCompletionService>("QWen3_Intent", ...);
builder.Services.AddKeyedSingleton<IChatCompletionService>("Ollama_Extraction", ...);
```

### Chat History Pattern
- Always compress history before processing queries
- History format: `Role: Content` pairs joined by newlines
- Use `ChatMessageContent` class for structured message handling

### Streaming Response Pattern
```csharp
// Use WriteSseMessage for structured event streaming
await WriteSseMessage(responseStream, "event-name", "message-data");
```

## Integration Points
1. **Environment Variables**:
   - `OPENAI_API_KEY` - OpenAI API key
   - `QWEN_API_KEY` - QWen API key
   
2. **External Services**:
   - OpenAI API (gpt-4)
   - QWen API (qwen3-72b-chat)
   - Local Ollama server (http://localhost:11434/v1)

## Common Operations
1. **Adding a New Agent**:
   - Register service in `Program.cs`
   - Add new Kernel instance in `MultiAgentService`
   - Implement processing in service pipeline
   
2. **Modifying Chat History Management**:
   - Adjust token threshold in `ChatHistoryService`
   - Modify summarization prompt if needed

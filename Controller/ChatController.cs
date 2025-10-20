using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using NashAI_app.Model;
using NashAI_app.Services;
using NashAI_app.utils;

namespace NashAI_app.Controller;

[ApiController]
[Route(ApiEndPoints.Chats.ChatsBase)]
public class ChatController: ControllerBase
{
    private readonly IChatClient _chatClient;
    private readonly SemanticSearch _semanticSearch;
    private readonly IRagService _ragService;

    public ChatController(IChatClient chatClient, SemanticSearch semanticSearch, IRagService ragService)
    {
        _chatClient = chatClient;
        _semanticSearch = semanticSearch;
        _ragService = ragService;
    }
    
    [HttpPost(ApiEndPoints.Chats.SEND_URL_CHATS)] 
    public async Task<IActionResult> SendMessageAsync([FromBody] ChatSessionModel session)
    {
        var userMessage = session.Messages.LastOrDefault(p => p.Role == ChatRole.User);
        if (userMessage == null)
            return BadRequest("User was not found");
    
        // Convert chat messages for AI client
        var chatMessages = session.Messages
            .Select(m => new ChatMessage(m.Role, m.MessageContent))
            .ToList();

        // Get assistant response
        var response = await _chatClient.GetResponseAsync(chatMessages);
        

        // Create assistant message
        var assistantMessage = new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            MessageContent = response?.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            SessionId = session.SessionId
        };

        // Add to session (in memory for now)
        session.Messages.Add(assistantMessage);

        // ✅ Return structured JSON that React expects
        return Ok(new
        {
            sessionId = session.SessionId,
            role = assistantMessage.Role.ToString(),
            messageContent = assistantMessage.MessageContent,
            createdAt = assistantMessage.CreatedAt
        });
    }

    [HttpPost(ApiEndPoints.Chats.SEMANTIC_SEARCH_URLS)]
    public async Task<IActionResult> SendSemanticSearch([FromBody] ChatSessionModel session)
    {
        var userMessage = session.Messages.LastOrDefault(p => p.Role == ChatRole.User);
        if (userMessage == null)
            return BadRequest("User was not found");
        
        // Perform Semantic Search for context
        var query = userMessage.MessageContent;
        
       var retrievedDocs = await _semanticSearch.SearchAsync(query, null, 4);
      
        // Build Context text from top results
        var contextText = string.Join("\n\n", retrievedDocs.Select(r => r.Text));
        
        // Add system prompt to include context
        var systemPrompt = new ChatMessage(ChatRole.System, 
            $"You are a helpful assistant. Use the following context to answer the question.\n\nContext:\n{contextText}");
        
        // Combine system + chat history + user message
        var chatMessages = new List<ChatMessage> { systemPrompt };
        chatMessages.AddRange(session.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));
        
        // Get AI response
        var response = await _chatClient.GetResponseAsync(chatMessages);
        
        // Create assistant message
        var assistantMessage = new ChatMessageModel
        {
            Role = ChatRole.Assistant,
            MessageContent = response?.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            SessionId = session.SessionId
        };

        // Add to session (in memory for now)
        session.Messages.Add(assistantMessage);

        // ✅ Return structured JSON that React expects
        return Ok(new
        {
            sessionId = session.SessionId,
            role = assistantMessage.Role.ToString(),
            messageContent = assistantMessage.MessageContent,
            createdAt = assistantMessage.CreatedAt,
            sources = retrievedDocs.Select(r => new
            {
                r.DocumentId,
                r.PageNumber,
                snippet = r.Text.Length > 250 ? r.Text[..250] + "..." : r.Text
            })
        });
    }
    
    
    [HttpGet(ApiEndPoints.Chats.SEARCH_URL_CHATS)]
    public async Task<IActionResult> SearchAsyncMessage([FromQuery] string query, [FromQuery] string? filesystem)
    {
        var results = await _semanticSearch.SearchAsync(query, filesystem, 5);
        return Ok(results);
    }
}
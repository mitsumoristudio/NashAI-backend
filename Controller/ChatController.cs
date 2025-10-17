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
    private readonly RagService _ragService;

    public ChatController(IChatClient chatClient, SemanticSearch semanticSearch, RagService ragService)
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

    [HttpPost(ApiEndPoints.Chats.SEND_SEMANTIC_SEARCH)]
    public async Task<IActionResult> SendSemanticMessageAsync([FromBody] ChatSessionModel session)
    {
        var result = await _ragService.GetRagResponseAsync(session);
        return Ok(new { session.SessionId, result });
    }
    
    
    [HttpGet(ApiEndPoints.Chats.SEARCH_URL_CHATS)]
    public async Task<IActionResult> SearchAsynceMessage([FromQuery] string query, [FromQuery] string? filesystem)
    {
        var results = await _semanticSearch.SearchAsync(query, filesystem, 10);
        return Ok(results);
    }
    
}
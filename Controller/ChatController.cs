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

    public ChatController(IChatClient chatClient, SemanticSearch semanticSearch)
    {
        _chatClient = chatClient;
        _semanticSearch = semanticSearch;
    }

    // Does not store in database or in memory
    [HttpPost(ApiEndPoints.Chats.SEND_URL_CHATS)]
    public async Task<IActionResult> SendMessageAsync([FromBody] ChatSessionModel session)
    {
        var userMessage = session.Messages.LastOrDefault(p => p.Role == ChatRole.User);
        if (userMessage == null)
            return BadRequest("User was not found");
        
    var chatMessage = session.Messages
        .Select(m => new ChatMessage(m.Role, m.MessageContent))
        .ToList();

    var response = await _chatClient.GetResponseAsync(chatMessage);
    
    session.Messages.Add(new ChatMessageModel
    {
        Role = ChatRole.Assistant,
        MessageContent = response.ToString(),
        CreatedAt = DateTime.UtcNow,
        SessionId = session.SessionId
    });
    
    return Ok(response);
    }
    
    [HttpGet(ApiEndPoints.Chats.SEARCH_URL_CHATS)]
    public async Task<IActionResult> SearchAsynceMessage([FromQuery] string query, [FromQuery] string? filesystem)
    {
        var results = await _semanticSearch.SearchAsync(query, filesystem, 100);
        return Ok(results);
    }
    
}
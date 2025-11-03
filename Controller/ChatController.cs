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
    private readonly SemanticSearchVB _semanticSearchVB;
   

    public ChatController(IChatClient chatClient, SemanticSearchVB semanticSearchVB)
    {
        _chatClient = chatClient;
        _semanticSearchVB = semanticSearchVB;

    }
    
    [HttpPost(ApiEndPoints.Chats.SEND_URL_CHATS)] 
    public async Task<IActionResult> SendMessageAsync([FromBody] ChatSessionVBModel sessionVb)
    {
        var userMessage = sessionVb.Messages.LastOrDefault(p => p.Role == ChatRole.User);
        if (userMessage == null)
            return BadRequest("User was not found");
    
        // Convert chat messages for AI client
        var chatMessages = sessionVb.Messages
            .Select(m => new ChatMessage(m.Role, m.MessageContent))
            .ToList();

        // Get assistant response
        var response = await _chatClient.GetResponseAsync(chatMessages);
        

        // Create assistant message
        var assistantMessage = new ChatMessageVBModel
        {
            Role = ChatRole.Assistant,
            MessageContent = response?.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            SessionId = sessionVb.SessionId
        };

        // Add to session (in memory for now)
        sessionVb.Messages.Add(assistantMessage);

        // ✅ Return structured JSON that React expects
        return Ok(new
        {
            sessionId = sessionVb.SessionId,
            role = assistantMessage.Role.ToString(),
            messageContent = assistantMessage.MessageContent,
            createdAt = assistantMessage.CreatedAt
        });
    }

    [HttpPost(ApiEndPoints.Chats.SEMANTIC_SEARCH_URLS)]
public async Task<IActionResult> SendSemanticSearch([FromBody] ChatSessionVBModel sessionVb)
{
    var userMessage = sessionVb.Messages.LastOrDefault(p => p.Role == ChatRole.User);
    if (userMessage == null)
        return BadRequest("User message not found.");

    // Call your vector search service
    var topResults = await _semanticSearchVB.SearchAsync(
        text: userMessage.MessageContent,
        documentId: null,
        maxResults: 5
    );

    if (!topResults.Any())
    {
        var assistantMessageEmpty = new ChatMessageVBModel
        {
            Role = ChatRole.Assistant,
            MessageContent = "I couldn’t find relevant information for your question in the available sources.",
            CreatedAt = DateTime.UtcNow,
            SessionId = sessionVb.SessionId
        };
        sessionVb.Messages.Add(assistantMessageEmpty);
        return Ok(new
        {
            sessionId = sessionVb.SessionId,
            role = assistantMessageEmpty.Role.ToString(),
            messageContent = assistantMessageEmpty.MessageContent,
            createdAt = assistantMessageEmpty.CreatedAt,
            sources = new object[] { }
        });
    }

    // Summarize top context before generating the assistant answer
   // var summary = await _semanticSearchVB.SummarizeResultsAsync(userMessage.MessageContent, topResults);
    
    // Give Legal Explanation 
    var summary = await _semanticSearchVB.AnalyzeContractClause(userMessage.MessageContent, topResults);
    
    // Combine retrieved context into system prompt
 //   var contextText = string.Join("\n\n---\n\n", topResults.Select(r => r.Content));
   
 // Added systemPrompt to use Semantic Search
//  var systemPrompt = $@"
// You are a precise and factual assistant.
// Use ONLY the following context to answer the user question.
// If the context does not contain enough information, say:
// 'I could not find relevant information in the provided sources.'
//
// ### Context:
// {contextText}";
 
// Gives overview of summarization
 var systemPrompt = $@"
You are an expert assistant using the following summary, overview or explanation to answer the question.
If the context does not contain enough information, say:
'I could not find relevant information in the provided sources.'

### Summary:
{summary}";

    // Prepare chat messages for AI client
    var chatMessages = new List<ChatMessage>
    {
        new ChatMessage(ChatRole.System, systemPrompt)
    };
    chatMessages.AddRange(sessionVb.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));

    // Get AI response
    var response = await _chatClient.GetResponseAsync(chatMessages);

    var assistantMessage = new ChatMessageVBModel
    {
        Role = ChatRole.Assistant,
        MessageContent = response?.ToString() ?? string.Empty,
        CreatedAt = DateTime.UtcNow,
        SessionId = sessionVb.SessionId
    };
    sessionVb.Messages.Add(assistantMessage);

    // Return JSON including sources
    return Ok(new
    {
        sessionId = sessionVb.SessionId,
        role = assistantMessage.Role.ToString(),
        messageContent = assistantMessage.MessageContent,
        createdAt = assistantMessage.CreatedAt,
        contextUsed = true,
        sources = topResults.Select(r => new
        {
            r.DocumentId,
            r.PageNumber,
            snippet = r.Content.Length > 250 ? r.Content[..250] + "..." : r.Content
        })
    });
}

    
    [HttpGet(ApiEndPoints.Chats.SEARCH_URL_CHATS)]
    public async Task<IActionResult> SearchAsyncMessage([FromQuery] string query, [FromQuery] string? filesystem)
    {
        var results = await _semanticSearchVB.SearchAsync(query, filesystem, 5);
        return Ok(results);
    }
}
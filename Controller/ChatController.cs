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
         bool IsRelevant(string text, string query, int minOverlap = 2)
        {
            var queryWords = query
                .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                .Select(w => w.ToLowerInvariant())
                .ToHashSet();

            var textWords = text
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.ToLowerInvariant());

            int overlap = textWords.Count(w => queryWords.Contains(w));
            return overlap >= minOverlap;
        }
         
        var userMessage = session.Messages.LastOrDefault(p => p.Role == ChatRole.User);
        if (userMessage == null)
            return BadRequest("User was not found");
        
        // Perform Semantic Search for context
        var query = userMessage.MessageContent;
        
       var retrievedDocs = await _semanticSearch.SearchAsync(query, null, 4);
       var relevantDocs = retrievedDocs.Where(d => d.Score >= 0.75).ToList();

       
       if (retrievedDocs.Count == 0)
       {
           return Ok(new
           {
               session.SessionId,
               role = ChatRole.Assistant.ToString(),
               messageContent = "I couldn’t find relevant information for your question in the available sources.",
               createdAt = DateTime.UtcNow,
               sources = new object[] { }
           });
       }
       
       string systemPrompt;
       bool contextUsed = retrievedDocs.Any();

       if (contextUsed)
       {
           // 🧩 Combine top chunks into one context block
           var contextText = string.Join("\n\n---\n\n", retrievedDocs.Select(r => r.Text));

           systemPrompt = @$"
You are a precise and factual assistant.
Use ONLY the following context to answer the user question.
If the context does not contain enough information, say:
'I could not find relevant information in the provided sources.'

### Context:
{contextText}";
       }
       else
       {
           // 🚫 No relevant context found
           systemPrompt = @"
You are a precise assistant.
There was no relevant information retrieved from the knowledge base.
Kindly inform the user that no relevant results were found.";
       }
       
        // Build Context text from top results
//         var contextText = string.Join("\n\n", retrievedDocs.Select(r => r.Text));
//         
//         // Add system prompt to include context
//         var systemPrompt = new ChatMessage(ChatRole.System,
//             @$"You are a precise and factual assistant. 
// Use ONLY the information from the provided context to answer.
// If the context does not contain enough detail, respond with:
// 'I could not find relevant information in the provided sources.'
//
// ### Context:
// {contextText}");
        
        // Combine system + chat history + user message
      //  var chatMessages = new List<ChatMessage> { systemPrompt };
      var chatMessages = new List<ChatMessage>
      {
          new ChatMessage(ChatRole.System, systemPrompt)
      };
      
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
        if (contextUsed)
        {
            return Ok(new
                   {
                       sessionId = session.SessionId,
                       role = assistantMessage.Role.ToString(),
                       messageContent = assistantMessage.MessageContent,
                       createdAt = assistantMessage.CreatedAt,
                       contextUsed = true,
                       sources = retrievedDocs.Select(r => new
                       {
                           r.DocumentId,
                           r.PageNumber,
                           snippet = r.Text.Length > 250 ? r.Text[..250] + "..." : r.Text
                       })
                   }); 
        } else {
            return Ok(new
            {
                sessionId = session.SessionId,
                role = assistantMessage.Role.ToString(),
                messageContent = assistantMessage.MessageContent,
                createdAt = assistantMessage.CreatedAt,
                contextUsed = false
            });
        }
       
    }
    
    [HttpGet(ApiEndPoints.Chats.SEARCH_URL_CHATS)]
    public async Task<IActionResult> SearchAsyncMessage([FromQuery] string query, [FromQuery] string? filesystem)
    {
        var results = await _semanticSearch.SearchAsync(query, filesystem, 5);
        return Ok(results);
    }
}
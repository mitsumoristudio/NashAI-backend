using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.AI;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class RagService : IRagService
{
    private readonly IChatClient _chatClient;
    private readonly SemanticSearchService _semanticSearchService;

    public RagService(IChatClient chatClient, SemanticSearchService semanticSearchService)
    {
        _chatClient = chatClient;
        _semanticSearchService = semanticSearchService;
    }

    public async Task<string> GetRagResponseAsync(ChatSessionVBModel sessionVb,  [FromQuery] string? filesystem)
    {
        var userMessage = sessionVb.Messages.LastOrDefault(role => role.Role == ChatRole.User);
        if (userMessage == null) return "No user was found";

        var retrievedDocs = await _semanticSearchService.SearchAsync(userMessage.MessageContent, filesystem, 5);
        var context = string.Join("\n\n", retrievedDocs.Select(d => d.Content));

        var systemMessage = new ChatMessage(ChatRole.System, $"Use this context:\n{context}");
        var chatMessages = new List<ChatMessage> { systemMessage };
        
        chatMessages.AddRange(sessionVb.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));

        var response = await _chatClient.GetResponseAsync(chatMessages);

        return response?.ToString() ??  "No response found";
    }

    public async Task<string> GenerateResponseAsync(string query, string sessionId)
    {
        // Retrieve Context
        var retrievedDocs = await _semanticSearchService.SearchAsync(query, null, 3);

        var contextBuilder = new StringBuilder();
        foreach (var doc in retrievedDocs)
        {
            contextBuilder.AppendLine(doc.Content);
            contextBuilder.AppendLine("\n---\n");
        }
        
        // Create prompt
        var systemPrompt = new ChatMessage(ChatRole.System,
            $"You are an AI assistant. Use the following context to answer accurately and concisely.\\n\\n{{contextBuilder}}");

        var userPrompt = new ChatMessage(ChatRole.User, query);
        
        var messages = new List<ChatMessage> { systemPrompt, userPrompt };
        
        // Generate a response
        var response = await _chatClient.GetResponseAsync(messages);
        
        return response?.ToString() ??  "No response was found";

    }
}
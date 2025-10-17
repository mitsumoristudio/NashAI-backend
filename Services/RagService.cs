using Microsoft.Extensions.AI;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class RagService
{
    private readonly IChatClient _chatClient;
    private readonly SemanticSearch _semanticSearch;

    public RagService(IChatClient chatClient, SemanticSearch semanticSearch)
    {
        _chatClient = chatClient;
        _semanticSearch = semanticSearch;
    }

    public async Task<string> GetRagResponseAsync(ChatSessionModel session)
    {
        var userMessage = session.Messages.LastOrDefault(role => role.Role == ChatRole.User);
        if (userMessage == null) return "No user was found";

        var retrievedDocs = await _semanticSearch.SearchAsync(userMessage.MessageContent, "N/A", 5);
        var context = string.Join("\n\n", retrievedDocs.Select(d => d.Text));

        var systemMessage = new ChatMessage(ChatRole.System, $"Use this context:\n{context}");
        var chatMessages = new List<ChatMessage> { systemMessage };
        
        chatMessages.AddRange(session.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));

        var response = await _chatClient.GetResponseAsync(chatMessages);
        return response?.ToString() ?? "No response found";
    }
}
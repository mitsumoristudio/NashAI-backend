using Microsoft.Extensions.AI;
using Nash_Manassas.Hub;
using NashAI_app.Model;

namespace NashAI_app.Services;

public class ProjectChatService: IProjectChatService
{
    private readonly IChatClient _chatClient;
    private readonly ProjectApiClients _projectClients;

    public ProjectChatService(IChatClient chatClient, ProjectApiClients projectClients)
    {
        _chatClient = chatClient;
        _projectClients = projectClients;
    }
    
    public async Task<ChatResponseDto> HandleProjectChatAsync(ChatSessionVBModel session)
    {
        var userMessage = session.Messages
            .Last(m => m.Role == ChatRole.User);
        
        // Fetch Project data from Postgres via API
        var fetchProjects = await _projectClients.ListProjectsAsync();
        
        // Reduce payload
        var projectContext = fetchProjects.Select(p => new
        {
            p.ProjectName,
            p.ProjectNumber,
            p.Contractor,
            p.Location,
            p.ProjectManager,
            p.ProjectEstimate,
            p.Description
        });
        
        // Build a system prompt
        var systemPrompt = BuildProjectSystemPrompt(projectContext: projectContext);
        
        // Build a chat Message
        var chatMessages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        chatMessages.AddRange(
            session.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));
        
        // Call the AI
        var response = await _chatClient.GetResponseAsync(chatMessages);
        
        // Store the assistant message
        var assistantMessage = new ChatMessageVBModel
        {
            Role = ChatRole.Assistant,
            MessageContent = response?.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            SessionId = session.SessionId
        };
        
        session.Messages.Add(assistantMessage);
        
        return ChatResponseDto.From(session, assistantMessage);

    }
    
    private static string BuildProjectSystemPrompt(object projectContext)
    {
        return $"""
                You are a construction project assistant.

                You have access to the following project data
                retrieved from a PostgreSQL database.

                Use ONLY this data to answer questions.
                If the answer is not available, say you do not know.

                Project Data:
                {System.Text.Json.JsonSerializer.Serialize(projectContext)}

                Be concise and factual.
                """;
    }
    
    
}
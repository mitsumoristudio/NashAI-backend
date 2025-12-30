using Microsoft.Extensions.AI;
using NashAI_app.Model;
using NashAI_app.utils;

namespace NashAI_app.Services;

public class EquipmentChatService: IEquipmentChatService
{
    private readonly IChatClient _chatClient;
    private readonly EquipmentApiClient _equipmentClient;

    public EquipmentChatService(IChatClient chatClient, EquipmentApiClient equipmentClient)
    {
        _chatClient = chatClient;
        _equipmentClient = equipmentClient;
    }

    public async Task<ChatResponseDto> HandleEquipmentChatAsync(ChatSessionVBModel session)
    {
        var userMessage = session.Messages
            .LastOrDefault(m => m.Role == ChatRole.User);
        
        foreach (var msg in session.Messages)
        {
            Console.WriteLine($"ROLE: {msg.Role} | CONTENT: {msg.MessageContent}");
        }
        
        if (userMessage == null)
        {
            throw new ArgumentException("No user message found in chat session");
        }
        
        // Fetch Equipment data from POSTGRES
        var fetchEquipments = await _equipmentClient.ListEquipmentsAsync();
        
        // Reduce Payload
        var equipmentContent = fetchEquipments.Select(e => new
        {
            e.EquipmentName,
            e.EquipmentNumber,
            e.EquipmentType,
            e.InternalExternal,
            e.Supplier,
            e.Description,
            e.MonthlyCost
        });
        
        // Build a system prompt
        var systemPrompt = BuildEquipmentSystemPrompt(equipmentContext: equipmentContent);
        
        // Build a AI Chat Message
        var chatMessages = new List<ChatMessage>
        {
            new(ChatRole.System, systemPrompt)
        };
        
        chatMessages.AddRange(
            session.Messages.Select(m => new ChatMessage(m.Role, m.MessageContent)));
        
        // Call the AI ChatResponse
        var aiResponse = await _chatClient.GetResponseAsync(chatMessages);
        
        // Store the Assistant Message
        var assistantMessage = new ChatMessageVBModel
        {
            Role = ChatRole.Assistant,
            MessageContent = aiResponse?.ToString() ?? string.Empty,
            CreatedAt = DateTime.UtcNow,
            SessionId = session.SessionId
        };
        
        session.Messages.Add(assistantMessage);
        
        return ChatResponseDto.From(session, assistantMessage);
        
    }
    
    private static string BuildEquipmentSystemPrompt(object equipmentContext)
    {
        return $"""
                You are a construction project assistant.

                You have access to the following equipment data
                retrieved from a PostgreSQL database.

                Use ONLY this data to answer questions.
                If the answer is not available, say you do not know.

                Equipment Data:
                {System.Text.Json.JsonSerializer.Serialize(equipmentContext)}

                Be concise and factual.
                """;
    }
}
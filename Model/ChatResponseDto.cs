namespace NashAI_app.Model;

public class ChatResponseDto
{
    public string SessionId { get; set; }
    public string Role { get; set; } 
    public string MessageContent {get; set;}
    public DateTime CreatedAt { get; set; }

    public static ChatResponseDto From(
        ChatSessionVBModel session,
        ChatMessageVBModel message)
    {
        return new ChatResponseDto
        {
            SessionId = session.SessionId,
            Role = message.Role.ToString(),
            MessageContent = message.MessageContent,
            CreatedAt = message.CreatedAt
        };
    }
}
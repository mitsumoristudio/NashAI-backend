namespace NashAI_app.Model;

public class ChatSessionModel
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Title { get; set; } = "New Chat";
    public List<ChatMessageModel> Messages { get; set; } = new List<ChatMessageModel>();
    public int? TokenUser { get; set; }
}
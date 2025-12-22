namespace NashAI_app.Model;

public class ChatSessionVBModel
{
    public string SessionId { get; set; } = Guid.NewGuid().ToString();
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string Title { get; set; } = "New Chat";
    public List<ChatMessageVBModel> Messages { get; set; } = new();

}
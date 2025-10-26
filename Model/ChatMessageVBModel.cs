using Microsoft.Extensions.AI;

namespace NashAI_app.Model;

public class ChatMessageVBModel
{
    public int Id { get; set; }
    public string SessionId {get; set;} = Guid.NewGuid().ToString();
    public ChatRole Role { get; set; } = ChatRole.User; // set role as user
    public string MessageContent { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public bool ContextUsed { get; set; } = false;

    public List<ChatSourceDBModel> Sources { get; set; } = new();

}
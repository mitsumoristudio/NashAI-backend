namespace NashAI_app.Model;

public class ChatSourceDBModel
{
    public int Id { get; set; }
    
    public int MessageId { get; set; }

    public string DocumentId { get; set; } = default!;
    
    public int PageNumber { get; set; }
    
    public string Snippet { get; set; } = default!;
    
    // Store embeddings as float array
    public float[] Embeddings { get; set; } = default!;
}
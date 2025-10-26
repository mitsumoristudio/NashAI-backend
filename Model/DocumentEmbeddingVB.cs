using System.ComponentModel.DataAnnotations.Schema;

namespace NashAI_app.Model;
using Pgvector;

public class DocumentEmbeddingVB
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string DocumentId {get; set;} = string.Empty;
    
    public int PageNumber {get; set;}
    
    public string Content {get; set;} = string.Empty;

    [Column(TypeName = "vector(1536)")] 
    public Vector Embeddings { get; set; } = default;
}
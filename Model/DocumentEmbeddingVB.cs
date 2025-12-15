using System.ComponentModel.DataAnnotations.Schema;
using Pgvector;

namespace NashAI_app.Model;

[Table("document_embedding")]
public class DocumentEmbeddingVB
{
    public Guid Id { get; set; } = Guid.NewGuid();
    
    public string DocumentId {get; set;} = string.Empty;
    
    public int PageNumber {get; set;}
    
    public string Content {get; set;} = string.Empty;

    // Leave as default for Embeddings to work on frontend
    [Column(TypeName = "vector(1536)")] 
    public Vector Embeddings { get; set; } = default;
}
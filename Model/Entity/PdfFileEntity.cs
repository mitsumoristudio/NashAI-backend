namespace Project_Manassas.Model;

public class PdfFileEntity
{
    public required Guid Id { get; set; }
    
    public required string DocumentId { get; set; }
    
    public required string FileName { get; set; } 
    
    public string ContentType { get; set; } = "application/pdf";
    
    public required byte[] Data { get; set; } 
    
    public DateTime UploadedAt { get; set; } 
}
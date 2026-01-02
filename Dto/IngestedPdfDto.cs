namespace NashAI_app.Dto;

public class IngestedPdfDto
{
    public string DocumentId { get; set; } = default!;
    public int ChunkCount {get; set;}
    public int PageCount {get; set;}
}
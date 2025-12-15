using System.ComponentModel.DataAnnotations;

namespace NashAI_app.Model;

public class SearchResultModel
{
    
    public required string DocumentId {get; set;}
    public int PageNumber {get; set;}
    public string? Content {get; set;}
    public double? Score {get; set;}
}
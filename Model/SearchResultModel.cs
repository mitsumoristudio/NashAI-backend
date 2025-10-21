namespace NashAI_app.Model;

public class SearchResultModel
{
    public string DocumentId {get; set;}
    public int PageNumber {get; set;}
    public string Text {get; set;}
    public double? Score {get; set;}
}
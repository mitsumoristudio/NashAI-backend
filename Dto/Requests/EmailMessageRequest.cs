namespace Project_Manassas.Dto.Requests;

public class EmailMessageRequest
{
    public string To { get; set; } = null!;
    public string Subject { get; set; } = null!;
    public string Message { get; set; } = null!;
}
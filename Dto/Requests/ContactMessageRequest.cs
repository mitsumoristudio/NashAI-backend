namespace Nash_Manassas.Dto;

public class ContactMessageRequest
{
    public string SenderName { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string Message { get; set; } = null!;
}
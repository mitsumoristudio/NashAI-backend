namespace Project_Manassas.Dto.Responses;

public class LoginResponse
{
    public string Token { get; set; } = string.Empty;
    public Guid Id { get; set; }
    public string UserName  { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    
    public bool? IsAdmin { get; set; } = false;
}
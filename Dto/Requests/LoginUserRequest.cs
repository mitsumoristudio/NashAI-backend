namespace Project_Manassas.Dto.Requests;

public class LoginUserRequest
{
    public required string Email { get; set; }
    
    public required string Password { get; set; }
}
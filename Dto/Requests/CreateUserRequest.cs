namespace Project_Manassas.Dto.Requests;

public class CreateUserRequest
{
    public required string UserName { get; set; }
    
    public required string Password { get; set; } 
    
    public required string Email { get; set; } 
    
    public bool? IsAdmin { get; set; } = false;
}
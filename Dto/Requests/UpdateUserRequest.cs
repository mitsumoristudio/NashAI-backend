namespace Project_Manassas.Dto.Requests;

public class UpdateUserRequest
{
    public required string UserName { get; set; }
    
    public required string PasswordHash { get; set; } 
    
    public required string Email { get; set; } 
    
    public required Guid Id { get; set; }
}
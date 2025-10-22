using Project_Manassas.Model;

namespace Project_Manassas.Dto.Responses;

public class UserResponse
{
    public required string UserName { get; set; }
    
    public required string PasswordHash { get; set; } 
    
    public required string Email { get; set; } 
    
    public required Guid Id { get; set; }
    
    public bool? IsAdmin { get; set; } = false;
    
    public ICollection<ProjectEntity>? Projects { get; set; }

}
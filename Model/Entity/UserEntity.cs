namespace Project_Manassas.Model;

public class UserEntity
{
    public required string UserName { get; set; }
    
    public required string PasswordHash { get; set; } 
    
    public required string Email { get; set; } 
    
    public required Guid Id { get; set; }

    public bool? IsAdmin { get; set; } = false;
    
    public ICollection<ProjectEntity>? Projects { get; set; }
    
    public bool EmailConfirmed { get; set; } = false;
    
    public string? EmailVerificationToken { get; set; }
    
    public DateTime? EmailVerificationTokenExpiration { get; set; } 
    
    public string? PasswordResetToken { get; set; }
    
    public DateTime? PasswordResetTokenExpiration { get; set; }

}
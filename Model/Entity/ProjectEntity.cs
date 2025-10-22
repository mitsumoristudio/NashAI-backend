using System.ComponentModel.DataAnnotations;

namespace Project_Manassas.Model;

public class ProjectEntity
{
    public required Guid Id { get; set; }
    
    [MaxLength(100)]
    public required string ProjectName { get; set; }
    
    [MaxLength(200)]
    public required string Description { get; set; }
    
    [MaxLength(100)]
    public required string ProjectNumber { get; set; }
    
    [MaxLength(50)]
    public required string Location { get; set; }
    
    [MaxLength(50)]
    public string? Contractor { get; set; }
    
    [MaxLength(50)] 
    public decimal ProjectEstimate { get; set; }
    
    [MaxLength(50)]
    public required string ProjectManager { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid? UserId { get; set; }
    
    public UserEntity? User { get; set; }
    
}
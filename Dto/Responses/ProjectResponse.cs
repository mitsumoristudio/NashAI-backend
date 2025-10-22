using Project_Manassas.Model;

namespace Project_Manassas.Dto.Responses;

public class ProjectResponse
{
    public required Guid Id { get; set; }
    
    public required string ProjectName { get; set; }
    
    public required string Description { get; set; }
    
    public required string ProjectNumber {get; set;}
    
    public required string Location { get; set; }
    
    public string? Contractor {get; set;}
    
    public required decimal ProjectEstimate { get; set; }
    
    public required string ProjectManager { get; set; }

    public DateTime CreatedAt { get; set; }
    
    public Guid? UserId { get; set; }
    
}
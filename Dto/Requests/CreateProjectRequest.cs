using Project_Manassas.Model;

namespace Project_Manassas.Dto.Requests;

public class CreateProjectRequest
{
    public required string ProjectName { get; set; }
    
    public required string Description { get; set; }
    
    public required string ProjectNumber {get; set;}
    
    public required string Location { get; set; }
    
    public string? Contractor {get; set;}
    
    public required int ProjectEstimate { get; set; }
    
    public required string ProjectManager { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public Guid UserId { get; set; }
    
}
using Project_Manassas.Model;

namespace Project_Manassas.Dto.Requests;

public class UpdateProjectRequest
{
    public Guid Id { get; set; }
    
    public required string projectname { get; set; }
    
    public required string description { get; set; }
    
    public required string projectnumber {get; set;}
    
    public required string location { get; set; }
    
    public string? contractor {get; set;}
    
    public required int projectestimate { get; set; }
    
    public required string projectmanager { get; set; }
    
    public DateTime createdAt { get; set; } = DateTime.UtcNow;

    public Guid userId { get; set; }
    //public IFormFile? ImageFile { get; set; } = default;

}
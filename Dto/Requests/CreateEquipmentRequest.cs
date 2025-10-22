namespace Project_Manassas.Dto.Requests;

public class CreateEquipmentRequest
{
    public required string EquipmentName { get; set; }
    
    public required string EquipmentNumber { get; set; }
    
    public required string Supplier {get; set;}
    
    public required string EquipmentType {get; set;}
    
    public required string InternalExternal {get; set;}
    
    public required string Description { get; set; }
    
    public string? ProjectName { get; set; }
    
    public int MonthlyCost { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    public Guid? UserId { get; set; }
    
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
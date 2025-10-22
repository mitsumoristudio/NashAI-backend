using System.ComponentModel.DataAnnotations;

namespace Project_Manassas.Model;

public class EquipmentEntity
{
    public required Guid Id { get; set; }
    
    [MaxLength(100)]
    public required string EquipmentName { get; set; }
    
    [MaxLength(100)]
    public required string EquipmentNumber { get; set; }
    
    [MaxLength(100)]
    public required string Supplier { get; set; }
    
    [MaxLength(50)]
    public required string EquipmentType { get; set; }
    
    [MaxLength(50)]
    public required string InternalExternal {get; set;}
    
    [MaxLength(100)]
    public required string Description { get; set; }
    
    [MaxLength(100)]
    public string? ProjectName { get; set; }
    
    public int MonthlyCost { get; set; }
    
    public Guid? ProjectId { get; set; }
    
    public Guid? UserId { get; set; }
    
    public UserEntity? User { get; set; }
    
    public ProjectEntity? Projects { get; set; }
    
    public DateTime? CreatedAt { get; set; } = DateTime.Now;
}
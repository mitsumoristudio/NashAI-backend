using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Project_Manassas.Database;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Mapping;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public class EquipmentService: IEquipmentService
{
    private readonly ProjectContext _dbContext;

    public EquipmentService(ProjectContext dbContext)
    {
        _dbContext = dbContext;
    }
    
    // Get All Equipment
    public async Task<EquipmentsResponse> GetAllEquipmentsAsync()
    {
        var response = await _dbContext.Equipments.ToListAsync();

        return new EquipmentsResponse
        {
            Items = response.Select(p => ContractEquipmentMapping.MaptoEquipmentResponse(p))
        };
    }

    // Get Equipment by ID
    public async Task<EquipmentResponse> GetEquipmentByIdAsync(Guid id)
    {
        var response = await _dbContext.Equipments.FindAsync(id);
        return (response is null ? null : ContractEquipmentMapping.MaptoEquipmentResponse(response)) ?? throw new InvalidOperationException("Equipment not found");
    }

    public async Task<bool> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest equipmentUpdate)
    {
        var equipment = await _dbContext.Equipments.FindAsync(id);
        if (equipment == null) 
            throw new InvalidOperationException("Equipment not found");
        
        equipment.EquipmentName = equipmentUpdate.EquipmentName;
        equipment.EquipmentNumber = equipmentUpdate.EquipmentNumber;
        equipment.Supplier = equipmentUpdate.Supplier;
        equipment.EquipmentType = equipmentUpdate.EquipmentType;
        equipment.InternalExternal = equipmentUpdate.InternalExternal;
        equipment.Description = equipmentUpdate.Description;
        equipment.ProjectName = equipmentUpdate.ProjectName;
        equipment.MonthlyCost = equipmentUpdate.MonthlyCost;

        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    // Delete Equipment by ID
    public async Task<bool> DeleteEquipmentAsync(Guid id)
    {
        var equipment = await _dbContext.Equipments.FindAsync(id);
        if (equipment is null) return false;
        
        _dbContext.Equipments.Remove(equipment);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<EquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request)
    {
        var equipment = ContractEquipmentMapping.MaptoEquipmentModel(request);
        _dbContext.Equipments.Add(equipment);
        await _dbContext.SaveChangesAsync();

        return ContractEquipmentMapping.MaptoEquipmentResponse(equipment);
    }

    public async Task<List<EquipmentEntity>> GetEquipmentByUserIdAsync(Guid userId)
    {
        var equipments = _dbContext.Equipments.Where(item => item.UserId == userId).ToListAsync();
        return await equipments;
    }
}
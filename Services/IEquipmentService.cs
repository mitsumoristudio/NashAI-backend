using Microsoft.AspNetCore.Mvc;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public interface IEquipmentService
{
    Task<EquipmentsResponse> GetAllEquipmentsAsync();
    
    Task<EquipmentResponse> GetEquipmentByIdAsync(Guid id);
    
    Task<bool> UpdateEquipmentAsync(Guid id, UpdateEquipmentRequest request);
    
    Task<bool> DeleteEquipmentAsync(Guid id);
    
    Task<EquipmentResponse> CreateEquipmentAsync(CreateEquipmentRequest request);

    Task<List<EquipmentEntity>> GetEquipmentByUserIdAsync(Guid userId);
}
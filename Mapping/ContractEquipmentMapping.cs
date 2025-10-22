using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Project_Manassas.Mapping;

public static class ContractEquipmentMapping
{
    public static EquipmentEntity MaptoEquipmentModel(this CreateEquipmentRequest request)
    {
        return new EquipmentEntity
        {
            Id = Guid.NewGuid(),
            EquipmentName = request.EquipmentName,
            EquipmentNumber = request.EquipmentNumber,
            Supplier = request.Supplier,
            EquipmentType = request.EquipmentType,
            InternalExternal = request.InternalExternal,
            Description = request.Description,
            ProjectName = request.ProjectName,
            MonthlyCost = request.MonthlyCost,
            UserId = request.UserId,
            CreatedAt = request.CreatedAt,
        };
    }

    public static EquipmentResponse MaptoEquipmentResponse(this EquipmentEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new EquipmentResponse
        {
            Id = entity.Id,
            EquipmentName = entity.EquipmentName,
            EquipmentNumber = entity.EquipmentNumber,
            Supplier = entity.Supplier,
            EquipmentType = entity.EquipmentType,
            InternalExternal = entity.InternalExternal,
            Description = entity.Description,
            ProjectName = entity.ProjectName,
            MonthlyCost = entity.MonthlyCost,
            UserId = entity.UserId,
            CreatedAt = entity.CreatedAt,

        };
    }
}
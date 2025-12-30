namespace Project_Manassas.Dto.Responses;

public class EquipmentsResponse
{
    public required IEnumerable<EquipmentResponse> Items { get; init; } = Enumerable.Empty<EquipmentResponse>();
}
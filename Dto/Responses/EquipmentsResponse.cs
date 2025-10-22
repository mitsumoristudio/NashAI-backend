namespace Project_Manassas.Dto.Responses;

public class EquipmentsResponse
{
    public required IEnumerable<EquipmentResponse> Items { get; set; } = Enumerable.Empty<EquipmentResponse>();
}
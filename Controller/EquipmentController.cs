using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NashAI_app;
using NashAI_app.utils;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Service;

namespace Project_Manassas.Controller;

[ApiController]
[Route(ApiEndPoints.Equipments.CREATE_URL_EQUIPMENT_CONSTANT)]
public class EquipmentController: ControllerBase
{
    private readonly IEquipmentService _equipmentService;
    private readonly IWebHostEnvironment _environment;

    public EquipmentController(IEquipmentService equipmentService, IWebHostEnvironment environment)
    {
        _equipmentService = equipmentService;
        _environment = environment;
    }
    
    // GET/ api/equipments
    [HttpGet]
    public async Task<ActionResult<IEnumerable<EquipmentResponse>>> GetAllEquipmentsAsync()
    {
        var equipments = await _equipmentService.GetAllEquipmentsAsync();
        return Ok(equipments);
    }
    
    // GET /api/equipments/{id}
    [HttpGet(ApiEndPoints.Equipments.GET_URL_EQUIPMENT)]
    public async Task<ActionResult<EquipmentsResponse>> GetEquipmentAsync(Guid id)
    {
        var equipment = await _equipmentService.GetEquipmentByIdAsync(id);
        return equipment == null ? NotFound() : Ok(equipment);
    }
    
    // POST/api/equipments
    [HttpPost]
    public async Task<IActionResult> CreateEquipmentAsync([FromBody] CreateEquipmentRequest request)
    {
        var equipment = await _equipmentService.CreateEquipmentAsync(request);
        return Ok(equipment);
    }
    
    // DELETE api/equipment/{id}
    [HttpDelete(ApiEndPoints.Equipments.DELETE_URL_EQUIPMENT)]
    public async Task<IActionResult> DeleteEquipmentAsync([FromRoute] Guid id)
    {
        var equipment = await _equipmentService.DeleteEquipmentAsync(id);
        return equipment ? NoContent() : NotFound("Equipment not found");
    }

    // PUT api/equipment/{id}
    [HttpPut(ApiEndPoints.Equipments.UPDATE_URL_EQUIPMENT)]
    public async Task<IActionResult> UpdateEquipmentAsync([FromRoute] Guid id,
        [FromBody] UpdateEquipmentRequest request)
    {
        var equipmentExists = await _equipmentService.UpdateEquipmentAsync(id, request);
        
       // if (equipmentExists == null) return NotFound();
        return Ok(equipmentExists);
    }
    
    // GET UserId api/equipments/user/{userid}
    [HttpGet(ApiEndPoints.Equipments.GET_URL_MYEQUIPMENT)]
    public async Task<ActionResult<IEnumerable<EquipmentsResponse>>> GetEquipmentsByUserIdAsync([FromRoute] Guid userId)
    {
        var equipments = await _equipmentService.GetEquipmentByUserIdAsync(userId);
        if (equipments == null) return NotFound("Equipment not found. Please contact admin");
        return Ok(equipments);
    }
    
}
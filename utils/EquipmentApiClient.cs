using System.Text.Json;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace NashAI_app.utils;

public class EquipmentApiClient
{
    private readonly HttpClient _httpClient;

    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public EquipmentApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        {/* for local development*/}
        //  _httpClient.BaseAddress = new Uri("http://localhost:5000/");

        if (_httpClient.BaseAddress == null)
        {
            var baseUrl = Environment.GetEnvironmentVariable("AZURE_WEB_API") ??
                          "https://nashai2-b2c3hhgwdwepcafk.eastus2-01.azurewebsites.net";
          
            _httpClient.BaseAddress = new Uri(baseUrl);
        }
    }
    
    public async Task<List<EquipmentResponse>> ListEquipmentsAsync()
    {
        var response = await _httpClient.GetAsync("api/equipments");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var wrapper = await JsonSerializer.DeserializeAsync<EquipmentsResponse>(stream, _jsonOptions);
        return (List<EquipmentResponse>)wrapper?.Items! ?? new List<EquipmentResponse>();;
        
    }

    public async Task<EquipmentResponse> FindEquipmentByNameAsync(string name)
    {
        var url = $"api/equipments/search?equipmentName={Uri.EscapeDataString(name)}";
        var response = await _httpClient.GetAsync(url);
        
        if (!response.IsSuccessStatusCode)
            throw new HttpRequestException($"Equipment search returned {response.StatusCode}");

        var json = await response.Content.ReadAsStringAsync();
        
        // Deserialize as list
        var equipments = JsonSerializer.Deserialize<EquipmentResponse>(json, _jsonOptions);
        
        // Return equipment
        return equipments ?? throw new InvalidOperationException();
    }

    public async Task<EquipmentEntity> CreatEquipmentAsync(CreateEquipmentRequest request)
    {
        var newEquipment = new EquipmentEntity
        {
            Id = Guid.NewGuid(),
            EquipmentName = request.EquipmentName,
            EquipmentNumber = request.EquipmentNumber,
            Supplier = request.Supplier,
            EquipmentType = request.EquipmentType,
            InternalExternal = request.InternalExternal,
            Description = request.EquipmentName,
        };
        
        var jsonContent = new StringContent(JsonSerializer.Serialize(newEquipment, _jsonOptions),
            System.Text.Encoding.UTF8, "application/json");
        
        var response = await _httpClient.PostAsync("api/equipments", jsonContent);
        response.EnsureSuccessStatusCode();
        
        var responseString = await response.Content.ReadAsStringAsync();
        var createEquipment = JsonSerializer.Deserialize<EquipmentEntity>(responseString, _jsonOptions);
        
        return createEquipment ?? throw new InvalidOperationException();
        
    }
    
}
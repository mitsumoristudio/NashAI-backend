using System.Diagnostics;
using System.Text.Json;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Nash_Manassas.Hub;

public class ProjectApiClients
{
    private readonly HttpClient _httpClient;
    
    private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    public ProjectApiClients(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("http://localhost:5000/");
    }

    public async Task<List<ProjectResponse>> ListProjectsAsync()
    {
        var response = await _httpClient.GetAsync("api/projects");
        response.EnsureSuccessStatusCode();

        var stream = await response.Content.ReadAsStreamAsync();
        var wrapper = await JsonSerializer.DeserializeAsync<ProjectsResponse>(stream, _jsonOptions);
        return (List<ProjectResponse>)wrapper?.Items ?? new List<ProjectResponse>();
    }
    
    public async Task<ProjectResponse> FindProjectByNameAsync(string name)
    {
        var url = $"api/projects/search?projectName={Uri.EscapeDataString(name)}";
        var response = await _httpClient.GetAsync(url);

        if (!response.IsSuccessStatusCode)
            return null;

        var json = await response.Content.ReadAsStringAsync();
        
        // Deserialize as list
        var projects = JsonSerializer.Deserialize<ProjectResponse>(json, _jsonOptions);
        
        // Return project
        return projects;
        
    }

    public async Task<ProjectEntity> CreateProjectAsync(CreateProjectRequest request)
    {
      
        var newProject = new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            Contractor = request.Contractor,
            CreatedAt = DateTime.UtcNow,
            ProjectName = request.ProjectName,
            ProjectNumber = request.ProjectNumber,
            Location = request.Location,
            ProjectManager = request.ProjectManager,
        };
        var jsonContent = new StringContent(JsonSerializer.Serialize(newProject, _jsonOptions), System.Text.Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync("api/projects", jsonContent);
        response.EnsureSuccessStatusCode();

        var responseString = await response.Content.ReadAsStringAsync();
        var createproject = JsonSerializer.Deserialize<ProjectEntity>(responseString, _jsonOptions);
        
        return createproject;
    }
}
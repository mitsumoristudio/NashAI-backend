using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NashAI_app;
using NashAI_app.Model;
using NashAI_app.Services;
using NashAI_app.utils;
using Project_Manassas.Database;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Mapping;
using Project_Manassas.Model;
using Project_Manassas.Service;


namespace Project_Manassas.Controller;

// API Version 1.0 August 26, 2025
[ApiController]
[Route(ApiEndPoints.Projects.CREATE_URL_PROJECTS_CONSTANT)]
public class ProjectController:  ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IWebHostEnvironment _environment;
    private readonly IProjectChatService _projectChatService;

    public ProjectController(IProjectService projectservice, IWebHostEnvironment environment, IProjectChatService projectChatService)
    {
      _projectService = projectservice;
      _environment = environment;
      _projectChatService = projectChatService;
    }
    
    // GET/ api/products
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ProjectsResponse>>> GetAllProjectsAsync()
    {
        var projects = await _projectService.GetAllProjectsAsync();
        return Ok(projects);
    }
    
    // GET /api/projects/{id}
    [HttpGet(ApiEndPoints.Projects.GET_URL_PROJECTS)]
    public async Task<ActionResult<ProjectResponse>> GetProjectAsync(Guid id)
    {
      var project = await _projectService.GetProjectAsync(id);
        return project == null ? NotFound("Project was not found") : Ok(project);
    }
    
    /* TODO Go back to fix the 404 error in HTTP PUT update Project */
    // PUT? api/projects/{id}
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> UpdateProjectAsync([FromRoute] Guid id, [FromBody] UpdateProjectRequest request)
    {
        var projectExists = await _projectService.UpdateProjectAsync(id, request);
        if (!projectExists) return NotFound("Project was not found. Please contact admin.");
        return NoContent();
    }
    
    // DELETE api/projects/{id}
    [HttpDelete(ApiEndPoints.Projects.DELETE_URL_PROJECTS)]
    public async Task<IActionResult> DeleteProjectAsync([FromRoute] Guid id)
    {
      var project = await _projectService.DeleteProjectAsync(id);
      return project ? NoContent() : NotFound("Project was not found.");
    }
    
    // GET UserId api/projects/user/{userid}
    [HttpGet(ApiEndPoints.Projects.GET_URL_MYPROJECT)]
    public async Task<ActionResult<IEnumerable<ProjectsResponse>>> GetProjectsByUserIdAsync([FromRoute] Guid userId)
    {
        var projects = await _projectService.GetProjectsByUserIdAsync(userId);
        if (projects == null ) return NotFound("Project was not found. Please contact admin.");
        return Ok(projects);
    }
    
    // POST/ add Images
  [HttpPost]
  public async Task<IActionResult> CreateProjectAsync([FromBody] CreateProjectRequest request)
  {
   var project = await _projectService.CreateProjectAsync(request, _environment.WebRootPath);
   return Ok(project);
  }
  
  // GET/ SEARCH BY Project Name
  [HttpGet(ApiEndPoints.Projects.SEARCH_BY_NAME)]
  public async Task<ActionResult<ProjectResponse>> GetProjectByNameAsync([FromQuery] string projectName)
  {
      if (string.IsNullOrWhiteSpace(projectName))
          return BadRequest("Project name is required.");
      
      var project = await _projectService.GetProjectByNameAsync(projectName);
      
      if (project == null)
          return NotFound(new { message = $"No project found with name '{projectName}'" });
      
      return Ok(project);
  }
  
  // POST ProjectChatService
  [HttpPost(ApiEndPoints.Projects.HandleProjectChat)]
  public async Task<IActionResult> HandleProjectChat([FromBody] ChatSessionVBModel session)
  {
      var project = await _projectChatService.HandleProjectChatAsync(session: session);
      
      return Ok(project);
  }
}
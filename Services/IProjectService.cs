using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public interface IProjectService
{
    Task<ProjectsResponse> GetAllProjectsAsync();
    
    Task<ProjectResponse> GetProjectAsync(Guid id);

    Task<bool> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    
    Task<bool> DeleteProjectAsync(Guid id);
    
    Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string uploadsRootPath);
    
    Task<List<ProjectEntity>> GetProjectsByUserIdAsync(Guid userId);

    Task<ProjectResponse?> GetProjectByNameAsync(string projectName);

}

/*
 * Benefit to using Service Layer
 * Controller is thin => Handles HTTP concerns only
 * Service layer handles the business logic - easier to test without HTTP
 * File handling and DB logic are encapsulated -> No duplication in controller
 * Scalable => if later you add logging, validation or cloud storage, you just change the service
 */

using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using Project_Manassas.Database;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Mapping;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public class ProjectService: IProjectService
{
    private readonly ProjectContext _dbContext;

    public ProjectService(ProjectContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<ProjectsResponse> GetAllProjectsAsync()
    {
        var projects = await _dbContext.Projects.ToListAsync();

        return new ProjectsResponse
        {
            Items = projects.Select(p => ContractProjectMapping.MaptoProjectResponse(p))
        };
    }

    public async Task<ProjectResponse> GetProjectAsync(Guid id)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        return (project is null ? null : ContractProjectMapping.MaptoProjectResponse(project)) ?? throw new InvalidOperationException();
    }

    public async Task<bool> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        var project = await _dbContext.Projects 
            .FirstOrDefaultAsync(p => p.Id == id);
        if (project == null) return false;
        
        project.ProjectName = request.projectname;
        project.Description = request.description;
        project.Contractor = request.contractor;
        project.ProjectEstimate = request.projectestimate;
        project.Location = request.location;
        project.ProjectManager = request.projectmanager;
        project.ProjectNumber = request.projectnumber;
        project.UserId = request.userId;
        
        await _dbContext.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        var project = await _dbContext.Projects.FindAsync(id);
        if (project is null) return false;
        
        _dbContext.Projects.Remove(project);
        await _dbContext.SaveChangesAsync();
        return true;
    }

    public async Task<ProjectResponse> CreateProjectAsync(CreateProjectRequest request, string uploadsRootPath)
    {
            var uploadsFolder = Path.Combine(uploadsRootPath, "uploads");

            // Create the upload folder if it doesn't exist
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }
            
            await _dbContext.SaveChangesAsync();

            // Save the Changes to the image file
        
        var project = ContractProjectMapping.MaptoProjectModel(request);

        _dbContext.Projects.Add(project);
        await _dbContext.SaveChangesAsync();

        return ContractProjectMapping.MaptoProjectResponse(project);
    }

    public async Task<List<ProjectEntity>> GetProjectsByUserIdAsync(Guid userId)
    {
        var projects = _dbContext.Projects.Where(p => p.UserId == userId).ToListAsync();
        return await projects;
        
    }

    public async Task<ProjectResponse?> GetProjectByNameAsync(string projectName)
    {
        // Convert lower case for case-insensitive comparison
        var project = await _dbContext.Projects
            .AsQueryable()
            .FirstOrDefaultAsync(p => EF.Functions.Like(p.ProjectName.ToLower(), projectName.ToLower()));
        
        return (project is null ? null : ContractProjectMapping.MaptoProjectResponse(project));
    }
}
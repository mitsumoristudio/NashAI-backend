using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Project_Manassas.Mapping;

public static class ContractProjectMapping
{
    public static ProjectEntity MaptoProjectModel(this CreateProjectRequest request)
    {
        return new ProjectEntity
        {
            Id = Guid.NewGuid(),
            Description = request.Description,
            ProjectNumber = request.ProjectNumber,
            ProjectEstimate = request.ProjectEstimate,
            Location = request.Location,
            ProjectManager = request.ProjectManager,
       //     ImageFile = request.ImageFile,
            ProjectName = request.ProjectName,
            CreatedAt = request.CreatedAt,
            Contractor = request.Contractor,
            UserId = request.UserId,
        };
    }

    public static ProjectsResponse MaptoProjectsResponse(this IEnumerable<ProjectEntity> projects)
    {
        return new ProjectsResponse
        {
            Items = projects.Select(MaptoProjectResponse)
        };
    }

    public static ProjectEntity MaptoProject(this UpdateProjectRequest request, Guid id)
    {
        return new ProjectEntity
        {
            Id = id,
            Description = request.description,
            ProjectNumber = request.projectnumber,
            ProjectEstimate = request.projectestimate,
            Location = request.location,
            ProjectManager = request.projectmanager,
           // ImageFile = request.ImageFile,
            ProjectName = request.projectname,
            Contractor = request.contractor,
            CreatedAt = request.createdAt,
        };
    }

    public static ProjectResponse MaptoProjectResponse(this ProjectEntity entity)
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));

        return new ProjectResponse
        {
            Id = entity.Id,
            Description = entity.Description,
            ProjectNumber = entity.ProjectNumber,
            ProjectName = entity.ProjectName,
            Location = entity.Location,
            Contractor = entity.Contractor,
            CreatedAt = entity.CreatedAt,
            ProjectManager = entity.ProjectManager,
            ProjectEstimate = entity.ProjectEstimate,
            UserId = entity.UserId,
        };
    }
    private static string SaveFile(IFormFile file)
    {
        var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var filePath = Path.Combine("wwwroot/uploads", fileName);

        Directory.CreateDirectory(Path.GetDirectoryName(filePath)!);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            file.CopyTo(stream);
        }

        // Return the relative path so it can be used in HTML <img src="">
        return $"/uploads/{fileName}";
    }

    
}
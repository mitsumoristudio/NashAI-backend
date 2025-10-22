namespace Project_Manassas.Dto.Responses;

public class ProjectsResponse
{
    public required IEnumerable<ProjectResponse> Items { get; init; } = Enumerable.Empty<ProjectResponse>();
}
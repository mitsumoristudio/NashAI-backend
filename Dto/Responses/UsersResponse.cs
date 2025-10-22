using Project_Manassas.Model;

namespace Project_Manassas.Dto.Responses;

public class UsersResponse
{
    public required IEnumerable<UserResponse> Users { get; init; } = Enumerable.Empty<UserResponse>();
}
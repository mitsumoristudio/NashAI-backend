using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;

namespace Project_Manassas.Mapping;

public static class ContractUserMapping
{
    public static UserEntity MaptoUserModel(this CreateUserRequest request)
    {
        return new UserEntity
        {
            Id = Guid.NewGuid(),
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = request.Password,
        };
    }

    public static UserResponse MaptoUserResponse(this UserEntity user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        return new UserResponse
        {
            Id = user.Id,
            UserName = user.UserName,
            Email = user.Email,
            PasswordHash = user.PasswordHash,
            IsAdmin = user.IsAdmin,
        };
    }

    public static UsersResponse MaptoUsersResponse(this IEnumerable<UserEntity> users)
    {
        return new UsersResponse
        {
            Users = users.Select(MaptoUserResponse)
        };
    }

    public static UserEntity MaptoUpdateUser(this UpdateUserRequest request, Guid id)
    {
        return new UserEntity
        {
            Id = id,
            UserName = request.UserName,
            Email = request.Email,
            PasswordHash = request.PasswordHash,
        };
    }
}
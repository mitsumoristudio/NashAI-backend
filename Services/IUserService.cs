using Microsoft.AspNetCore.Mvc;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;

namespace Project_Manassas.Service;

public interface IUserService
{
    Task<UsersResponse> GetAllUsersAsync();
    
    Task<UserResponse> GetUserAsync(Guid id);

    Task<bool> DeleteAsyncUser(Guid id);

    Task<LoginResponse?> LoginAsyncUser(LoginUserRequest userlogin);

    Task<bool> UpdateAsyncUser(string? currentUserName, UpdateUserRequest? userUpdate);
    
    Task<RegistrationResponse?> RegisterAsyncUser(CreateUserRequest userCreate);

}
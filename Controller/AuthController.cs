using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using NashAI_app;
using NashAI_app.utils;
using Project_Manassas.Database;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Model;
using Project_Manassas.Service;

namespace Project_Manassas.Controller;

[ApiController]
[Route(ApiEndPoints.Users.Base)]

public class AuthController : ControllerBase
{
    private readonly IUserService _userService;

    public AuthController(IUserService userService)
    {
        _userService = userService;
    }
    
    // POST/ api/users/register
    [HttpPost(ApiEndPoints.Users.REGISTER_URL_USER_CONSTANT)]
    public async Task<ActionResult<UserEntity>> RegisterAsyncUser([FromBody] CreateUserRequest user)
    {
        var create = await _userService.RegisterAsyncUser(user);
        
        if (create == null) return BadRequest("Email is already in use.");
        
        return Ok(create);
    }
    
    // POST/ api/users/{id}
    [Authorize]
    [HttpPut(ApiEndPoints.Users.UPDATE_URL_USER_CONSTANT)]
    public async Task<IActionResult> UpdateAsyncUser(UpdateUserRequest userUpdate)
    {
        var userName = User.Identity?.Name;
        var updated = await _userService.UpdateAsyncUser(userName, userUpdate);
        
        if (!updated) return BadRequest("User was not found.");
        
        return Ok(updated);
    }
    
    // PUT/ api/users/login
    [HttpPost(ApiEndPoints.Users.LOGIN_URL_USER_CONSTANT)]
    public async Task<IActionResult> LoginAsyncUser(LoginUserRequest userlogin)
    {
        // var token = await _userService.LoginAsyncUser(userlogin);
        //
        // if (token == null) return BadRequest("User was not found.");
        
     //   return Ok(new { token });
     
     var result = await _userService.LoginAsyncUser(userlogin);
     if (result == null) return Unauthorized();
     
     return Ok(result);
    }
    
    // DELETE/ api/users/{id}
    [Authorize]
    [HttpDelete(ApiEndPoints.Users.DELETE_URL_USER_CONSTANT)]
    public async Task<IActionResult> DeleteAsyncUser(Guid id)
    {
        var user = await _userService.DeleteAsyncUser(id);
        if (!user) return BadRequest("User was not found.");
        
        return Ok();
    }

    // POST/ api/users/logout
    [HttpPost(ApiEndPoints.Users.LOGOUT_URL_USER_CONSTANT)]
    public IActionResult LogOut()
    {
        return Ok(new { message = "You logged out." });
    }
    
    // GET/api/users
    [HttpGet]
    public async Task<ActionResult<IEnumerable<UsersResponse>>> GetAllUsersAsync()
    {
        var users = await _userService.GetAllUsersAsync();
        return Ok(users);
    }
    
    // GET /api/users/{id}
    [HttpGet(ApiEndPoints.Users.GET_URL_USER_CONSTANT)]
    public async Task<ActionResult<UserResponse>> GetUserAsync(Guid id)
    {
        var user = await _userService.GetUserAsync(id);
        return user is null ? NotFound("User was not found") : Ok(user);
    }
}
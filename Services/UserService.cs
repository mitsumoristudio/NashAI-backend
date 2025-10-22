using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Project_Manassas.Database;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Dto.Responses;
using Project_Manassas.Mapping;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public class UserService : IUserService
{
   private readonly ProjectContext _dbContext;
   private readonly JwtSettings _jwtSettings;

   public UserService(ProjectContext dbContext, IOptions<JwtSettings> jwtSettings)
   {
      _dbContext = dbContext;
      _jwtSettings = jwtSettings.Value;
   }

   public async Task<UsersResponse> GetAllUsersAsync()
   {
      var projects = await _dbContext.Users.ToListAsync();

      return new UsersResponse
      {
         Users = projects.Select(p => ContractUserMapping.MaptoUserResponse(p))
      };
   }

   public async Task<UserResponse> GetUserAsync(Guid id)
   {
      var user = await _dbContext.Users.FindAsync(id);
      
      return (user is null ? null : ContractUserMapping.MaptoUserResponse(user)) ?? throw new InvalidOperationException();
   }
   
   public async Task<bool> DeleteAsyncUser(Guid id)
   {
      var user = await _dbContext.Users.FindAsync(id);
      if (user == null) return false;
      
      _dbContext.Users.Remove(user);
      await _dbContext.SaveChangesAsync();
      return true;
   }

   public async Task<LoginResponse?> LoginAsyncUser(LoginUserRequest userlogin)
   {
      var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == userlogin.Email);
      
      if  (user == null) return null;
      
      if (!BCrypt.Net.BCrypt.Verify(userlogin.Password, user.PasswordHash)) return null;
      
      // Create claims for JWT
      var claims = new[]
      {
         new Claim(ClaimTypes.Name, user.UserName),
         new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
      };
      
      // Prepare the JWT signing
    //  var secretKey = Convert.FromBase64String(_jwtSettings.SecretKey);
      var secretKey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
      var key = new SymmetricSecurityKey(secretKey);
      var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        
      // Create JWT Token
      var token = new JwtSecurityToken(
         issuer: _jwtSettings.Issuer,
         audience: _jwtSettings.Audience,
         claims: claims,
         expires: DateTime.UtcNow.AddMinutes(_jwtSettings.ExpiryMinutes),
         signingCredentials: credentials);
      
    //  return new JwtSecurityTokenHandler().WriteToken(token);
    var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

    // create JSON Response
    return new LoginResponse
    {
       Token = tokenString,
       Id = user.Id,
       UserName = user.UserName,
       Email = user.Email,
       IsAdmin = user.IsAdmin
    };
   }

   public async Task<bool> UpdateAsyncUser(string? currentUserName, UpdateUserRequest? userUpdate)
   {
      var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.UserName == currentUserName);
      if (user == null) return false;

      if (userUpdate != null)
      {
         user.Email = userUpdate.Email;
         user.PasswordHash = userUpdate.PasswordHash;

         if (!string.IsNullOrWhiteSpace(userUpdate.PasswordHash))
         {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(userUpdate.PasswordHash);
         }
      }

      await _dbContext.SaveChangesAsync();
      return true;
   }

   public async Task<RegistrationResponse?> RegisterAsyncUser(CreateUserRequest user)
   {
      if (await _dbContext.Users.AnyAsync(p => p.Email == user.Email))
      {
         return null;
      }
      
      var newUser = new UserEntity
      {
         Id = Guid.NewGuid(),
         UserName = user.UserName,
         Email = user.Email,
         PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password)
      };
        
      _dbContext.Users.Add(newUser);
      await _dbContext.SaveChangesAsync();

      return new RegistrationResponse
      {
         Id = newUser.Id,
         UserName = newUser.UserName,
         Email = newUser.Email
      };
   }
}


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
    private readonly ProjectContext _dbContext;
    private readonly IEmailSenderService _emailSenderService;
    private readonly IEmailVerificationService _emailVerificationService;

    public AuthController(IUserService userService, ProjectContext dbContext, IEmailSenderService emailSenderService, IEmailVerificationService emailVerificationService)
    {
        _userService = userService;
        _dbContext = dbContext;
        _emailSenderService = emailSenderService;
        _emailVerificationService = emailVerificationService;
    }
    
    // POST/ api/users/register
    [HttpPost(ApiEndPoints.Users.REGISTER_URL_USER_CONSTANT)]
    public async Task<ActionResult<UserEntity>> RegisterAsyncUser([FromBody] CreateUserRequest user)
    {
        var create = await _userService.RegisterAsyncUser(user);
        
        if (create == null) return BadRequest("Email is already in use.");
        
        return Ok(create);
    }
    
    // POST/ api/users/registerEmail
    [HttpPost(ApiEndPoints.Users.REGISTER_URL_USER_W_EMAIL)]
    public async Task<ActionResult<UserEntity>> RegisterEmailUser([FromBody] CreateUserRequest user)
    {
        var createUser = await _userService.RegisterEmailUser(user);
        
        if (createUser == null) return BadRequest("Email is already in use.");
        
        // Send verification code
      
        await _emailVerificationService.SendVerificationCode(user.Email);
        
       
        return Ok(new { message = "User created successfully. Verification email has been sent." });
        //return Ok(createUser);
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
    
    // POST /api/users/verify-email
    [HttpPost("verifyEmail")]
    public async Task<IActionResult> VerifyEmailAsync(VerifyEmailRequest request)
    {
        var isValid = await _emailVerificationService.VerifyCodeAsync(request.Email, request.Code);
        
        if (!isValid) return BadRequest("Invalid or expired code.");
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        if (user != null) user.EmailConfirmed = true;

        await _dbContext.SaveChangesAsync();
        
        return Ok(new {message = "Email verified successfully. Verification email has been sent." });
    }
    
    [HttpPost("forgotPassword")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.Email == request.Email);
        
        if (user == null) return BadRequest("User was not found.");
        
        // Create Reset Token
        user.PasswordResetToken = Guid.NewGuid().ToString();
        user.PasswordResetTokenExpiration = DateTime.UtcNow.AddMinutes(30);
        
        await _dbContext.SaveChangesAsync();
        
      //  var resetUrl =  $"https://morisolution.org/resetPassword?token={user.PasswordResetToken}";
        var resetUrl = $"http://localhost:3000/resetPassword?token={user.PasswordResetToken}";
        
        await _emailSenderService.SendEmailAsync(
            request.Email,
            "Reset Your Password",
            BuildEmailTempplate(verifyUrl:resetUrl)
        );
        
        return Ok(new {message = "Your password reset email was sent"});
    }
    
    // Resend Code
    [HttpPost("resendCode")]
    public async Task<IActionResult> ResendCode([FromQuery] string email)
    {
        await _emailVerificationService.SendVerificationCode(email);
        return Ok(new {message = "Verification code was resent."});
    }
    
    // POST /api/users/resetPassword
    [HttpPost("resetPassword")]
    public async Task<IActionResult> ResetPasswordAsync(string token, NewPasswordRequest request)
    {
        var user = await _dbContext.Users.SingleOrDefaultAsync(u => u.PasswordResetToken == token);

        if (user == null || user.PasswordResetTokenExpiration < DateTime.UtcNow)
        {
            return BadRequest("Invalid or Expired Token");
        }
        
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.newPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiration = null;

        await _dbContext.SaveChangesAsync();
        
        return Ok(new {message = "Password reset was successful."});
    }

    private string BuildEmailTempplate(string verifyUrl)
    {
        return $@"
<!DOCTYPE html>
<html lang=""en"" style=""margin:0;padding:0;background:#f5f6fa;"">
<head>
    <meta charset=""UTF-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
    <title>Password Reset</title>
</head>

<body style=""margin:0;padding:0;background:#f5f6fa;font-family:Arial,Helvetica,sans-serif;"">

    <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background:#f5f6fa;padding:40px 0;"">
        <tr>
            <td align=""center"">

                <!-- Card container -->
                <table width=""600"" cellpadding=""0"" cellspacing=""0"" 
                       style=""background:#ffffff;border-radius:12px;overflow:hidden;
                              box-shadow:0 4px 16px rgba(0,0,0,0.1);"">

                    <!-- Header -->
                    <tr>
                        <td style=""background:#4a6cf7;padding:24px;text-align:center;color:white;"">
                            <h2 style=""margin:0;font-size:24px;font-weight:600;"">Password Reset Request</h2>
                        </td>
                    </tr>

                    <!-- Body -->
                    <tr>
                        <td style=""padding:32px 30px;color:#333;font-size:16px;line-height:1.6;"">

                            <p style=""margin:0 0 20px;"">
                                Hello,
                            </p>

                            <p style=""margin:0 0 20px;"">
                                We received a request to reset your password for your
                                <strong>Mori Solution</strong> account.
                            </p>

                            <p style=""margin:0 0 30px;"">
                                If you made this request, click the button below to set a new password.
                            </p>

                            <!-- Button -->
                            <table cellpadding=""0"" cellspacing=""0"" style=""margin:0 auto 32px auto;"">
                                <tr>
                                    <td align=""center"" 
                                        style=""background:#4a6cf7;color:white;padding:14px 28px;
                                               border-radius:6px;text-decoration:none;"">
                                        <a href=""{verifyUrl}"" 
                                           style=""color:white;text-decoration:none;font-size:16px;font-weight:bold;"">
                                            Reset Password
                                        </a>
                                    </td>
                                </tr>
                            </table>

                            <p style=""margin:0 0 20px;"">
                                Or copy and paste this link into your browser:
                            </p>

                            <p style=""word-break:break-all;color:#4a6cf7;font-size:14px;margin-bottom:32px;"">
                                {verifyUrl}
                            </p>

                            <p style=""margin:0 0 20px;"">
                                This password reset link will expire in <strong>30 minutes</strong>.
                            </p>

                            <p style=""margin:0;"">
                                If you did not request a password reset, you can safely ignore this email.
                            </p>
                        </td>
                    </tr>

                    <!-- Footer -->
                    <tr>
                        <td style=""background:#f0f2f5;padding:20px;text-align:center;color:#555;font-size:12px;"">
                            <p style=""margin:0 0 5px;"">© {DateTime.UtcNow.Year.ToString()} Mori Solution. All rights reserved.</p>
                            <p style=""margin:0;"">This is an automated message — please do not reply.</p>
                        </td>
                    </tr>

                </table>

            </td>
        </tr>
    </table>

</body>
</html>";
    }


}
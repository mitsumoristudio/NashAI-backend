using Microsoft.AspNetCore.Mvc;
using Nash_Manassas.Dto;
using NashAI_app.utils;
using Project_Manassas.Dto.Requests;
using Project_Manassas.Service;

namespace Nash_Manassas.Controller;

[ApiController]
[Route(ApiEndPoints.Messages.Base)]
public class MessageController: ControllerBase
{
    private readonly IEmailSenderService  _emailSenderService;

    public MessageController(IEmailSenderService emailSenderService)
    {
        _emailSenderService = emailSenderService;
    }
    
    // -------------------------
    // SendGrid Send Email Implementation
    // -------------------------
    [HttpPost(ApiEndPoints.Messages.CREATE_URL_MESSAGE)]
    public async Task<IActionResult> SendEmailAsync([FromBody] EmailMessageRequest emailRequest)
    {
        await _emailSenderService.SendEmailAsync(
            emailRequest.To,
            emailRequest.Subject,
            emailRequest.Message
            );
        
        return Ok(new { message = "Email sent successfully" });
    }
    
    // -------------------------
    // Send Contact Form Message (Goes to your admin email)
    // -------------------------
    [HttpPost(ApiEndPoints.Messages.SEND_CONTACT_MESSAGE)]
    public async Task<IActionResult> SendContactAsync([FromBody] ContactMessageRequest contactRequest)
    {
        var body = $@"
            <h3>New Contact Form Submission</h3>
            <p><strong>Name:</strong> {contactRequest.SenderName}</p>
            <p><strong>Email:</strong> {contactRequest.SenderEmail}</p>
            <p><strong>Message:</strong></p>
            <p>{contactRequest.Message}</p>";

        await _emailSenderService.SendEmailAsync(
            "runprospectpark@gmail.com",
            "New Contact Form Message",
            body
        );
        
        return Ok(new { message = "Message was sent to Admin" });
    }
    
    // -------------------------
    // Send VerificationEmail
    // -------------------------
    [HttpPost(ApiEndPoints.Messages.SEND_VERIFICATION_EMAIL)]
    public async Task<IActionResult> SendVerificationEmailAsync([FromBody] string userEmail)
    {
        var token = Guid.NewGuid().ToString();
        var verifyUrl = $"https://morisolution.org/verifyEmail?token={token}";

        var body = $@"
            <h2>Verify Your Email</h2>
            <p>Please click the link below to verify your account:</p>
            <a href='{verifyUrl}'>Verify Email</a>
        ";
        await _emailSenderService.SendEmailAsync(userEmail, "Verification Email", body);
        
        return Ok(new { message = "Verification email sent successfully" });
    }
    
}
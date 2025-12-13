using Microsoft.EntityFrameworkCore;
using Project_Manassas.Database;
using Project_Manassas.Model;

namespace Project_Manassas.Service;

public class EmailVerificationService : IEmailVerificationService
{
    private readonly ProjectContext _dbContext;
    private readonly IEmailSenderService _emailSenderService;

    public EmailVerificationService(ProjectContext dbContext, IEmailSenderService emailSenderService)
    {
        _dbContext = dbContext;
        _emailSenderService = emailSenderService;
    }
    
    public async Task SendVerificationCode(string email)
    {
        // Invalidate old codes
        var oldCodes = _dbContext.VerificationCodes.Where(x => x.Email == email && !x.Used);
        _dbContext.VerificationCodes.RemoveRange(oldCodes);
        
        // Generate 6 point random number
        var random6Number = new Random().Next(100000, 999999).ToString();

        var newEntity = new VerificationCode
        {
            Email = email,
            Code = random6Number,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10)
        };

        await _dbContext.VerificationCodes.AddAsync(newEntity);
        await _dbContext.SaveChangesAsync();
        
       //    var verifyUrl = $"https://morisolution.org/verifyEmail?token={random6Number}";
         var verifyUrl = $"http://localhost:3000/verifyEmail?email={email}";       
        
        var subject = "Your Verification Code";
        // var body = $"Your verification code is: {random6Number}\n\nIt expires in 10 minutes." +
        //                  $"Click the link to verify your email: <a href='{verifyUrl}'>Verify</a>";
        var body = BuildEmailTempplate(random6Number, verifyUrl);
     
        await _emailSenderService.SendEmailAsync(email, subject, body);
    }

    public async Task<bool> VerifyCodeAsync(string email, string code)
    {
        var records = await _dbContext.VerificationCodes
            .Where(x => x.Email == email && !x.Used)
            .OrderByDescending(x => x.ExpiresAt)
            .FirstOrDefaultAsync();
        
        if (records == null) return false;
        
        if (records.ExpiresAt < DateTime.UtcNow) return false;

        records.AttemptCount++;
        if (records.AttemptCount > 5) return false;

        if (records.Code != code)
        {
            await _dbContext.SaveChangesAsync();
            return false;
        }
        
        records.Used = true;
        await _dbContext.SaveChangesAsync();
        return true;
    }

    private string BuildEmailTempplate(string code, string verifyUrl)
    {
        return $@"
<!DOCTYPE html>
<html>
<head>
  <meta charset='UTF-8' />
  <style>
    body {{
        font-family: 'Segoe UI', Arial, sans-serif;
        background-color: #f5f7fa;
        padding: 0;
        margin: 0;
    }}
    .container {{
        max-width: 520px;
        margin: 30px auto;
        background: #ffffff;
        border-radius: 10px;
        padding: 40px 35px;
        box-shadow: 0 4px 12px rgba(0,0,0,0.07);
    }}
    .header {{
        font-size: 20px;
        font-weight: 600;
        color: #333333;
        margin-bottom: 25px;
        text-align: center;
    }}
    .code {{
        font-size: 32px;
        font-weight: bold;
        text-align: center;
        letter-spacing: 8px;
        color: #2d7df6;
        background-color: #eef4ff;
        padding: 15px;
        border-radius: 8px;
        margin: 25px 0;
    }}
    p {{ color: #555555; font-size: 15px; line-height: 1.5; }}
    .verify-btn {{
        display: block;
        margin: 20px auto;
        width: fit-content;
        padding: 12px 20px;
        background: #2d7df6;
        color: white !important;
        text-decoration: none;
        font-weight: 600;
        border-radius: 8px;
    }}
    .footer {{
        text-align: center;
        margin-top: 30px;
        color: #999999;
        font-size: 12px;
    }}
  </style>
</head>
<body>

<div class='container'>
    <div class='header'>Verify Your Email Address</div>

    <p>Enter the verification code below to continue:</p>

    <div class='code'>{code}</div>

    <a class='verify-btn' href='{verifyUrl}'>Verify Email</a>

    <p>This code expires in <strong>10 minutes</strong>.  
       If you did not request this, you can ignore this email.</p>

    <div class='footer'>© {DateTime.UtcNow.Year} Morisolution.org — All rights reserved.</div>
</div>

</body>
</html>";
    }
}

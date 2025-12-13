using SendGrid;
using SendGrid.Helpers.Mail;

namespace Project_Manassas.Service;

public class SendGridEmailService: IEmailSenderService
{
   
    public async Task SendEmailAsync(string to, string subject, string body)
    {
        var sendGridApiKey = Environment.GetEnvironmentVariable("SENDGRID_API_KEY");
       var sendGridClient = new SendGridClient(sendGridApiKey);
        var message = new SendGridMessage()
        {
            From = new SendGrid.Helpers.Mail.EmailAddress("mitsumori@nashmanassas.org", "Mori Solution"),
            Subject = subject,
            HtmlContent = body,
        };
        
        message.AddTo(new EmailAddress(to));
        
        await sendGridClient.SendEmailAsync(message);
        
    }
}
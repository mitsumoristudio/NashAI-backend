namespace Project_Manassas.Service;

public interface IEmailSenderService
{
    Task SendEmailAsync(string to, string subject, string body);
}
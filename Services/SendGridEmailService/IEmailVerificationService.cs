namespace Project_Manassas.Service;

public interface IEmailVerificationService
{
    Task SendVerificationCode(string email);
    Task<bool> VerifyCodeAsync(string email, string code);
}


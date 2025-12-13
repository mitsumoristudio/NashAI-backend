namespace Project_Manassas.Model;

public class VerificationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; }
    public string Code { get; set; } = null!;
    public DateTime ExpiresAt { get; set; }
    public bool Used { get; set; } = false;
    
    public int AttemptCount { get; set; } = 0; // security
}
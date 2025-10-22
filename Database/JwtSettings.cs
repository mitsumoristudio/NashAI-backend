namespace Project_Manassas.Database;

public class JwtSettings
{
    public required string SecretKey  { get; set; }
    public required string Issuer { get; set; }
    public required string Audience { get; set; }
    public required double ExpiryMinutes { get; set; }

}
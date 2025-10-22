namespace Project_Manassas.Dto.Responses;

public class RegistrationResponse
{
    public Guid Id { get; set; }
    
    public required string UserName { get; set; }
    
    public required string Email { get; set; }
}
using System.ComponentModel.DataAnnotations;

namespace Project_Manassas.Dto.Requests;

public class ForgotPasswordRequest
{
    
    public required string Email { get; set; }
}
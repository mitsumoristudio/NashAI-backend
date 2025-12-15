using System.ComponentModel.DataAnnotations;

namespace Project_Manassas.Dto.Requests;

public class VerifyEmailRequest
{

    public required string Email { get; set; }
    
  
    public required string Code { get; set; }
}
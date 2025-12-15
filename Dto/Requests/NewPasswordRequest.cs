using System.ComponentModel.DataAnnotations;

namespace Project_Manassas.Dto.Requests;

public class NewPasswordRequest
{
    public required string newPassword { get; set; }
}
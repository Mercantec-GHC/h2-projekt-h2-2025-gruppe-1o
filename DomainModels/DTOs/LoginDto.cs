using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class LoginDto
    {
        [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
        [Required(ErrorMessage = "Email er påkrævet")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adgangskode er påkrævet")]
        public string Password { get; set; } = string.Empty;
    }
}
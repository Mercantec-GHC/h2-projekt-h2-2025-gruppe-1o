using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Fornavn er påkrævet")]
        public string FirstName { get; set; } = string.Empty;
        [Required(ErrorMessage = "Efternavn er påkrævet")]
        public string LastName { get; set; } = string.Empty;
        [EmailAddress(ErrorMessage = "Ugyldig email adresse")]
        [Required(ErrorMessage = "Email er påkrævet")]
        public string Email { get; set; } = string.Empty;
        [Required(ErrorMessage = "Adgangskode er påkrævet")]
        [MinLength(8, ErrorMessage = "Adgangskoden skal være mindst 8 tegn lang")]
        public string Password { get; set; } = string.Empty;
        [Required(ErrorMessage = "Bekræft venligst din adgangskode")]
        [Compare("Password", ErrorMessage = "Adgangskoderne er ikke ens")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
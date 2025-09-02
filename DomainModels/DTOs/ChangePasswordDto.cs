using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Nuværende adgangskode er påkrævet")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Ny adgangskode er påkrævet")]
        [MinLength(8, ErrorMessage = "Den nye adgangskode skal være mindst 8 tegn lang")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Bekræft venligst den nye adgangskode")]
        [Compare("NewPassword", ErrorMessage = "De nye adgangskoder er ikke ens")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
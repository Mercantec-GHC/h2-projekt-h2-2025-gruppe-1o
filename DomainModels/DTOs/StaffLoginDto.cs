using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class StaffLoginDto
    {
        [Required(ErrorMessage = "Brugernavn er påkrævet")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Adgangskode er påkrævet")]
        public string Password { get; set; } = string.Empty;
    }
}
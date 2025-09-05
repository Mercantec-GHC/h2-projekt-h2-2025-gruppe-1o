using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class UserUpdateDto
    {
        [Required(ErrorMessage = "Fornavn er påkrævet")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Efternavn er påkrævet")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email er påkrævet")]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? PhoneNumber { get; set; } // TILFØJET
    }
}
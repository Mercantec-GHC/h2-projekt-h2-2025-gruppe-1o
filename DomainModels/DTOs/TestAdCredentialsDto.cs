using System.ComponentModel.DataAnnotations;

namespace DomainModels.DTOs
{
    public class TestAdCredentialsDto
    {
        [Required]
        public string Username { get; set; } = string.Empty;

        [Required]
        public string Password { get; set; } = string.Empty;
    }
}
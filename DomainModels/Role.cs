using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// Repræsenterer en brugerrolle i systemet, som definerer et sæt af tilladelser.
    /// </summary>
    public class Role : Common
    {
        /// <summary>
        /// Det unikke navn på rollen (f.eks. "Admin", "User").
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// En kort beskrivelse af, hvad rollen indebærer.
        /// </summary>
        [MaxLength(200)]
        public string? Description { get; set; }

        /// <summary>
        /// Navigation property til de brugere, der har denne rolle.
        /// </summary>
        public virtual ICollection<User> Users { get; set; } = new List<User>();

        /// <summary>
        /// Indeholder konstante streng-værdier for rollenavne for at undgå "magic strings" i koden.
        /// </summary>
        public static class Names
        {
            public const string User = "User";
            public const string CleaningStaff = "CleaningStaff";
            public const string Reception = "Reception";
            public const string Admin = "Admin";
        }
    }
}
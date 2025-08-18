using System;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    /// <summary>
    /// En baseklasse for alle database-entiteter, der indeholder fælles egenskaber.
    /// </summary>
    public class Common
    {
        /// <summary>
        /// Den unikke identifikator for entiteten (Primary Key).
        /// </summary>
        [Key]
        public string Id { get; set; }

        /// <summary>
        /// Tidspunktet, hvor entiteten blev oprettet i databasen.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// Tidspunktet, hvor entiteten sidst blev opdateret.
        /// </summary>
        public DateTime UpdatedAt { get; set; }
    }
}
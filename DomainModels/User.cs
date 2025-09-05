using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DomainModels
{
    public class User : Common
    {
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;
        [MaxLength(20)]
        public string PhoneNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string HashedPassword { get; set; } = string.Empty;
        public string? Salt { get; set; }
        public DateTime LastLogin { get; set; }
        public new string PasswordBackdoor { get; set; } = string.Empty;
        public string RoleId { get; set; } = string.Empty;
        public virtual Role? Role { get; set; }
    }
}
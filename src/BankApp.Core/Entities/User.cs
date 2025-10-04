#nullable enable
using System;

namespace BankApp.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = "Customer"; // Admin, Staff, Customer
        public string? FullName { get; set; }
        public bool IsActive { get; set; } = true;

        // Fields for email verification
        public string? VerificationCode { get; set; }
        public DateTime? VerificationCodeExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}


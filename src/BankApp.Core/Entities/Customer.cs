#nullable enable
using System;

namespace BankApp.Core.Entities
{
    public class Customer : BaseEntity
    {
        public int UserId { get; set; } // Foreign Key to Users table
        public string IdentityNumber { get; set; } = string.Empty; // TC No
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
    }
}


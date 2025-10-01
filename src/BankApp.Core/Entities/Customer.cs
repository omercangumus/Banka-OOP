using System;

namespace BankApp.Core.Entities
{
    public class Customer : BaseEntity
    {
        public int UserId { get; set; } // Foreign Key to Users table
        public string IdentityNumber { get; set; } // TC No
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PhoneNumber { get; set; }
        public string Email { get; set; } // Can be same as User.Email or different
        public string Address { get; set; }
        public DateTime? DateOfBirth { get; set; }
        
        // Navigation property logic usually handled by ORM, 
        // keeping it simple for Dapper as just IDs usually.
    }
}

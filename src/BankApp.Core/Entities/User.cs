namespace BankApp.Core.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; }
        public string PasswordHash { get; set; }
        public string Email { get; set; }
        public string Role { get; set; } // Admin, Staff, Customer
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

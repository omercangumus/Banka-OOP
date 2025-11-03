using System;

namespace BankApp.Core.Entities
{
    public class QuickContact : BaseEntity
    {
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string IBAN { get; set; } = string.Empty;
        public string ColorHex { get; set; } = "#3B82F6"; // Default Blue
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}

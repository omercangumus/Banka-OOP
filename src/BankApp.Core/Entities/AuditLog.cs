using System;

namespace BankApp.Core.Entities
{
    public class AuditLog : BaseEntity
    {
        public int? UserId { get; set; } // Nullable, as system might do actions
        public string Action { get; set; }
        public string Details { get; set; }
        public string IpAddress { get; set; }
    }
}

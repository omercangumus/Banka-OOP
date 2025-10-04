#nullable enable
using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Denetim logu entity sınıfı
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class AuditLog : BaseEntity
    {
        private int? _userId;
        private string _action = string.Empty;
        private string? _details;
        private string? _ipAddress;

        /// <summary>İşlemi yapan kullanıcı ID (Nullable, sistem işlemleri için)</summary>
        public int? UserId { get => _userId; set => _userId = value; }

        /// <summary>Yapılan İşlem</summary>
        public string Action { get => _action; set => _action = value; }

        /// <summary>İşlem Detayları</summary>
        public string? Details { get => _details; set => _details = value; }

        /// <summary>IP Adresi</summary>
        public string? IpAddress { get => _ipAddress; set => _ipAddress = value; }
    }
}

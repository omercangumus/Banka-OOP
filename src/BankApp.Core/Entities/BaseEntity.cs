using System;

namespace BankApp.Core.Entities
{
    /// <summary>
    /// Temel Entity sınıfı - Tüm entity'ler bu sınıftan türer
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public abstract class BaseEntity
    {
        private int _id;
        private DateTime _createdAt = DateTime.UtcNow;

        /// <summary>Birincil Anahtar (Primary Key)</summary>
        public int Id { get => _id; set => _id = value; }

        /// <summary>Kayıt Oluşturulma Tarihi</summary>
        public DateTime CreatedAt { get => _createdAt; set => _createdAt = value; }
    }
}

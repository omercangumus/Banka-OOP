using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Kredi kartı işlemleri servisi
    /// </summary>
    public class CardService
    {
        private readonly AccountRepository _accountRepo;
        private readonly AuditRepository _auditRepo;
        
        // In-memory kartlar (gerçek uygulamada DB'de olur)
        private static readonly List<CreditCard> _cards = new List<CreditCard>();
        private static readonly List<CardTransaction> _cardTransactions = new List<CardTransaction>();

        public CardService(AccountRepository accountRepo, AuditRepository auditRepo)
        {
            _accountRepo = accountRepo;
            _auditRepo = auditRepo;
        }

        /// <summary>
        /// Yeni sanal kart oluştur
        /// </summary>
        public CreditCard CreateVirtualCard(int customerId, string holderName, decimal limit)
        {
            var card = CreditCard.CreateVirtualCard(customerId, holderName, limit);
            card.Id = _cards.Count + 1;
            _cards.Add(card);
            return card;
        }

        /// <summary>
        /// Fiziksel kart oluştur
        /// </summary>
        public CreditCard CreatePhysicalCard(int customerId, string holderName, decimal limit, string colorTheme = "Gold")
        {
            var card = CreditCard.CreateVirtualCard(customerId, holderName, limit);
            card.Id = _cards.Count + 1;
            card.CardType = "Physical";
            card.ColorTheme = colorTheme;
            _cards.Add(card);
            return card;
        }

        /// <summary>
        /// Müşterinin kartlarını getir
        /// </summary>
        public List<CreditCard> GetCustomerCards(int customerId)
        {
            return _cards.Where(c => c.CustomerId == customerId && c.IsActive).ToList();
        }

        /// <summary>
        /// Kart borcu öde
        /// </summary>
        public async Task<(bool Success, string Message)> PayDebtAsync(int cardId, int accountId, decimal amount)
        {
            try
            {
                var card = _cards.FirstOrDefault(c => c.Id == cardId);
                if (card == null)
                    return (false, "Kart bulunamadı");

                if (amount <= 0)
                    return (false, "Geçersiz tutar");

                if (amount > card.CurrentDebt)
                    amount = card.CurrentDebt; // Borçtan fazla ödeme yapılamaz

                // Hesap bakiyesini kontrol et
                var account = await _accountRepo.GetByIdAsync(accountId);
                if (account == null)
                    return (false, "Hesap bulunamadı");

                if (account.Balance < amount)
                    return (false, $"Yetersiz bakiye. Mevcut: {account.Balance:N2} TL");

                // Bakiyeden düş, borcu azalt
                account.Balance -= amount;
                await _accountRepo.UpdateAsync(account);

                card.CurrentDebt -= amount;
                card.AvailableLimit += amount;

                // Transaction kaydet
                _cardTransactions.Add(new CardTransaction
                {
                    CardId = cardId,
                    Amount = -amount,
                    Description = "Borç Ödemesi",
                    TransactionDate = DateTime.Now,
                    TransactionType = "Payment"
                });

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = card.CustomerId,
                    Action = "CardPayment",
                    Details = $"Kart *{card.CardNumber.Substring(12)} için {amount:N2} TL borç ödendi",
                    IpAddress = "127.0.0.1"
                });

                return (true, $"{amount:N2} TL borç ödendi. Kalan borç: {card.CurrentDebt:N2} TL");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Harcama simülasyonu
        /// </summary>
        public (bool Success, string Message) SimulateSpending(int cardId, decimal amount, string merchant)
        {
            try
            {
                var card = _cards.FirstOrDefault(c => c.Id == cardId);
                if (card == null)
                    return (false, "Kart bulunamadı");

                if (!card.IsActive)
                    return (false, "Kart aktif değil");

                if (amount > card.AvailableLimit)
                    return (false, $"Yetersiz limit. Kullanılabilir: {card.AvailableLimit:N2} TL");

                // Harcama yap
                card.AvailableLimit -= amount;
                card.CurrentDebt += amount;

                // Transaction kaydet
                _cardTransactions.Add(new CardTransaction
                {
                    CardId = cardId,
                    Amount = amount,
                    Description = merchant,
                    TransactionDate = DateTime.Now,
                    TransactionType = "Purchase"
                });

                return (true, $"{merchant} - {amount:N2} TL harcama yapıldı");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Kart hareketlerini getir
        /// </summary>
        public List<CardTransaction> GetCardTransactions(int cardId)
        {
            return _cardTransactions
                .Where(t => t.CardId == cardId)
                .OrderByDescending(t => t.TransactionDate)
                .Take(20)
                .ToList();
        }

        /// <summary>
        /// Demo kartlar oluştur
        /// </summary>
        public void CreateDemoCards(int customerId, string holderName)
        {
            if (!_cards.Any(c => c.CustomerId == customerId))
            {
                // Gold fiziksel kart
                var goldCard = CreatePhysicalCard(customerId, holderName, 25000, "Gold");
                SimulateSpending(goldCard.Id, 3500, "Amazon.com.tr");
                SimulateSpending(goldCard.Id, 1200, "Migros");
                SimulateSpending(goldCard.Id, 850, "Netflix");

                // Sanal kart
                var virtualCard = CreateVirtualCard(customerId, holderName, 10000);
                SimulateSpending(virtualCard.Id, 500, "Steam");
                SimulateSpending(virtualCard.Id, 250, "Spotify");
            }
        }
    }

    /// <summary>
    /// Kart işlem kaydı
    /// </summary>
    public class CardTransaction
    {
        public int Id { get; set; }
        public int CardId { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = string.Empty;
        public DateTime TransactionDate { get; set; }
        public string TransactionType { get; set; } = string.Empty; // Purchase, Payment
    }
}

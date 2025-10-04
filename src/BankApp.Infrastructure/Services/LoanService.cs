using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Kredi işlemleri servisi
    /// </summary>
    public class LoanService
    {
        private readonly AccountRepository _accountRepo;
        private readonly AuditRepository _auditRepo;
        
        // In-memory loans (gerçek uygulamada DB'de olur)
        private static readonly List<Loan> _loans = new List<Loan>();
        private static int _nextId = 1;

        public LoanService(AccountRepository accountRepo, AuditRepository auditRepo)
        {
            _accountRepo = accountRepo;
            _auditRepo = auditRepo;
        }

        /// <summary>
        /// Kredi başvurusu yap
        /// </summary>
        public async Task<(bool Success, string Message)> ApplyForLoanAsync(int userId, int customerId, decimal amount, int termMonths, string? notes = null)
        {
            try
            {
                if (amount < 1000)
                    return (false, "Minimum kredi tutarı 1.000 TL'dir");

                if (termMonths < 3 || termMonths > 60)
                    return (false, "Vade 3-60 ay arasında olmalıdır");

                // Faiz oranı hesapla (vadeye göre)
                decimal interestRate = termMonths switch
                {
                    <= 12 => 3.0m,
                    <= 24 => 3.5m,
                    <= 36 => 4.0m,
                    _ => 4.5m
                };

                var loan = new Loan
                {
                    Id = _nextId++,
                    UserId = userId,
                    CustomerId = customerId,
                    Amount = amount,
                    TermMonths = termMonths,
                    InterestRate = interestRate,
                    Status = "Pending",
                    ApplicationDate = DateTime.Now,
                    Notes = notes
                };

                _loans.Add(loan);

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "LoanApplication",
                    Details = $"Kredi başvurusu: {amount:N2} TL, {termMonths} ay vade",
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Application");

                return (true, $"Kredi başvurunuz alındı! Aylık taksit: {loan.MonthlyPayment:N2} TL");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Bekleyen kredileri getir (Admin için)
        /// </summary>
        public List<Loan> GetPendingLoans()
        {
            return _loans.Where(l => l.Status == "Pending").OrderBy(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Tüm kredileri getir
        /// </summary>
        public List<Loan> GetAllLoans()
        {
            return _loans.OrderByDescending(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Kullanıcının kredilerini getir
        /// </summary>
        public List<Loan> GetUserLoans(int userId)
        {
            return _loans.Where(l => l.UserId == userId).OrderByDescending(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Krediyi onayla (Admin)
        /// </summary>
        public async Task<(bool Success, string Message)> ApproveLoanAsync(int loanId, int adminId)
        {
            try
            {
                var loan = _loans.FirstOrDefault(l => l.Id == loanId);
                if (loan == null)
                    return (false, "Kredi bulunamadı");

                if (loan.Status != "Pending")
                    return (false, "Bu kredi zaten işlenmiş");

                // Müşterinin hesabını bul ve kredi tutarını ekle
                var accounts = await _accountRepo.GetByCustomerIdAsync(loan.CustomerId);
                var mainAccount = accounts?.FirstOrDefault();
                
                if (mainAccount != null)
                {
                    mainAccount.Balance += loan.Amount;
                    await _accountRepo.UpdateAsync(mainAccount);
                }

                loan.Status = "Approved";
                loan.DecisionDate = DateTime.Now;
                loan.ApprovedById = adminId;

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminId,
                    Action = "LoanApproval",
                    Details = $"Kredi onaylandı: {loan.Amount:N2} TL (ID: {loanId})",
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Approved");

                return (true, $"Kredi onaylandı! {loan.Amount:N2} TL müşteri hesabına eklendi.");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Krediyi reddet (Admin)
        /// </summary>
        public async Task<(bool Success, string Message)> RejectLoanAsync(int loanId, int adminId, string reason)
        {
            try
            {
                var loan = _loans.FirstOrDefault(l => l.Id == loanId);
                if (loan == null)
                    return (false, "Kredi bulunamadı");

                if (loan.Status != "Pending")
                    return (false, "Bu kredi zaten işlenmiş");

                loan.Status = "Rejected";
                loan.DecisionDate = DateTime.Now;
                loan.ApprovedById = adminId;
                loan.RejectionReason = reason;

                // Audit log
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminId,
                    Action = "LoanRejection",
                    Details = $"Kredi reddedildi (ID: {loanId}). Sebep: {reason}",
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Rejected");

                return (true, "Kredi başvurusu reddedildi.");
            }
            catch (Exception ex)
            {
                return (false, $"Hata: {ex.Message}");
            }
        }

        /// <summary>
        /// Demo kredi başvuruları oluştur
        /// </summary>
        public void CreateDemoLoans()
        {
            if (!_loans.Any())
            {
                _loans.Add(new Loan
                {
                    Id = _nextId++,
                    UserId = 2, // musteri
                    CustomerId = 1,
                    Amount = 25000,
                    TermMonths = 12,
                    InterestRate = 3.0m,
                    Status = "Pending",
                    ApplicationDate = DateTime.Now.AddDays(-2),
                    Notes = "Araba alımı için"
                });

                _loans.Add(new Loan
                {
                    Id = _nextId++,
                    UserId = 2,
                    CustomerId = 1,
                    Amount = 50000,
                    TermMonths = 24,
                    InterestRate = 3.5m,
                    Status = "Pending",
                    ApplicationDate = DateTime.Now.AddDays(-1),
                    Notes = "Ev tadilat için"
                });
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Infrastructure.Data;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// Kredi işlemleri servisi
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class LoanService
    {
        private readonly AccountRepository _accountRepo;
        private readonly AuditRepository _auditRepo;
        
        // In-memory loans (gerçek uygulamada DB'de olur)
        private static readonly List<Loan> _loans = new List<Loan>();
        private static int _nextId = 1;

        /// <summary>
        /// LoanService yapıcı metodu
        /// </summary>
        /// <param name="accountRepo">Hesap repository</param>
        /// <param name="auditRepo">Denetim logu repository</param>
        public LoanService(AccountRepository accountRepo, AuditRepository auditRepo)
        {
            _accountRepo = accountRepo;
            _auditRepo = auditRepo;
        }

        /// <summary>
        /// Kredi başvurusu yap
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <param name="customerId">Müşteri ID</param>
        /// <param name="amount">Kredi tutarı</param>
        /// <param name="termMonths">Vade (ay)</param>
        /// <param name="notes">Notlar</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> ApplyForLoanAsync(int userId, int customerId, decimal amount, int termMonths, string? notes = null)
        {
            try
            {
                if (amount < 1000)
                {
                    return "Minimum kredi tutarı 1.000 TL'dir";
                }

                if (termMonths < 3 || termMonths > 60)
                {
                    return "Vade 3-60 ay arasında olmalıdır";
                }

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
                var sbAudit = new StringBuilder();
                sbAudit.Append("Kredi başvurusu: ");
                sbAudit.Append(amount.ToString("N2"));
                sbAudit.Append(" TL, ");
                sbAudit.Append(termMonths);
                sbAudit.Append(" ay vade");
                
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = userId,
                    Action = "LoanApplication",
                    Details = sbAudit.ToString(),
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Application");

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Kredi başvurusu hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Bekleyen kredileri getir (Admin için)
        /// </summary>
        /// <returns>Bekleyen kredi listesi</returns>
        public List<Loan> GetPendingLoans()
        {
            return _loans.Where(l => l.Status == "Pending").OrderBy(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Tüm kredileri getir
        /// </summary>
        /// <returns>Tüm kredi listesi</returns>
        public List<Loan> GetAllLoans()
        {
            return _loans.OrderByDescending(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Kullanıcının kredilerini getir
        /// </summary>
        /// <param name="userId">Kullanıcı ID</param>
        /// <returns>Kullanıcının kredi listesi</returns>
        public List<Loan> GetUserLoans(int userId)
        {
            return _loans.Where(l => l.UserId == userId).OrderByDescending(l => l.ApplicationDate).ToList();
        }

        /// <summary>
        /// Krediyi onayla (Admin)
        /// </summary>
        /// <param name="loanId">Kredi ID</param>
        /// <param name="adminId">Onaylayan admin ID</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> ApproveLoanAsync(int loanId, int adminId)
        {
            try
            {
                var loan = _loans.FirstOrDefault(l => l.Id == loanId);
                if (loan == null)
                {
                    return "Kredi bulunamadı";
                }

                if (loan.Status != "Pending")
                {
                    return "Bu kredi zaten işlenmiş";
                }

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
                var sbAudit = new StringBuilder();
                sbAudit.Append("Kredi onaylandı: ");
                sbAudit.Append(loan.Amount.ToString("N2"));
                sbAudit.Append(" TL (ID: ");
                sbAudit.Append(loanId);
                sbAudit.Append(")");
                
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminId,
                    Action = "LoanApproval",
                    Details = sbAudit.ToString(),
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Approved");

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Kredi onaylama hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Krediyi reddet (Admin)
        /// </summary>
        /// <param name="loanId">Kredi ID</param>
        /// <param name="adminId">Reddeden admin ID</param>
        /// <param name="reason">Red sebebi</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> RejectLoanAsync(int loanId, int adminId, string reason)
        {
            try
            {
                var loan = _loans.FirstOrDefault(l => l.Id == loanId);
                if (loan == null)
                {
                    return "Kredi bulunamadı";
                }

                if (loan.Status != "Pending")
                {
                    return "Bu kredi zaten işlenmiş";
                }

                loan.Status = "Rejected";
                loan.DecisionDate = DateTime.Now;
                loan.ApprovedById = adminId;
                loan.RejectionReason = reason;

                // Audit log
                var sbAudit = new StringBuilder();
                sbAudit.Append("Kredi reddedildi (ID: ");
                sbAudit.Append(loanId);
                sbAudit.Append("). Sebep: ");
                sbAudit.Append(reason);
                
                await _auditRepo.AddLogAsync(new AuditLog
                {
                    UserId = adminId,
                    Action = "LoanRejection",
                    Details = sbAudit.ToString(),
                    IpAddress = "127.0.0.1"
                });

                // Event fırlat
                AppEvents.NotifyDataChanged("Loan", "Rejected");

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Kredi reddetme hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
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
                    UserId = 2,
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

using System;
using System.Text;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    /// <summary>
    /// İşlem servisi - Para transferi ve işlem yönetimi
    /// Created by Fırat Üniversitesi Standartları, 01/01/2026
    /// </summary>
    public class TransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAuditRepository _auditRepository;

        /// <summary>
        /// TransactionService yapıcı metodu
        /// </summary>
        /// <param name="accountRepo">Hesap repository</param>
        /// <param name="transactionRepo">İşlem repository</param>
        /// <param name="auditRepo">Denetim logu repository</param>
        public TransactionService(IAccountRepository accountRepo, ITransactionRepository transactionRepo, IAuditRepository auditRepo)
        {
            _accountRepository = accountRepo;
            _transactionRepository = transactionRepo;
            _auditRepository = auditRepo;
        }

        /// <summary>
        /// Para transferi yapar
        /// </summary>
        /// <param name="fromAccountId">Kaynak hesap ID</param>
        /// <param name="toIban">Hedef IBAN</param>
        /// <param name="amount">Transfer tutarı</param>
        /// <param name="description">Açıklama</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> TransferMoneyAsync(int fromAccountId, string toIban, decimal amount, string description)
        {
            try
            {
                // Validasyonlar
                if (amount <= 0)
                {
                    return "Transfer tutarı 0'dan büyük olmalıdır.";
                }

                var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
                if (fromAccount == null)
                {
                    return "Kaynak hesap bulunamadı.";
                }

                var toAccount = await _accountRepository.GetByIBANAsync(toIban);
                if (toAccount == null)
                {
                    return "Hedef IBAN bulunamadı.";
                }

                if (fromAccount.Balance < amount)
                {
                    return "Yetersiz bakiye.";
                }

                // Borç işlemi (Debit)
                fromAccount.Balance -= amount;
                await _accountRepository.UpdateAsync(fromAccount);
                
                var sbDescOut = new StringBuilder();
                sbDescOut.Append("Transfer to ");
                sbDescOut.Append(toIban);
                sbDescOut.Append(": ");
                sbDescOut.Append(description);
                
                await _transactionRepository.AddAsync(new Transaction
                {
                    AccountId = fromAccount.Id,
                    TransactionType = "TransferOut",
                    Amount = amount,
                    Description = sbDescOut.ToString()
                });

                // Alacak işlemi (Credit)
                toAccount.Balance += amount;
                await _accountRepository.UpdateAsync(toAccount);
                
                var sbDescIn = new StringBuilder();
                sbDescIn.Append("Transfer from ");
                sbDescIn.Append(fromAccount.IBAN);
                sbDescIn.Append(": ");
                sbDescIn.Append(description);
                
                await _transactionRepository.AddAsync(new Transaction
                {
                    AccountId = toAccount.Id,
                    TransactionType = "TransferIn",
                    Amount = amount,
                    Description = sbDescIn.ToString()
                });

                // Audit Log
                var sbAudit = new StringBuilder();
                sbAudit.Append(amount.ToString("N2"));
                sbAudit.Append(" ");
                sbAudit.Append(fromAccount.CurrencyCode);
                sbAudit.Append(" from ");
                sbAudit.Append(fromAccount.AccountNumber);
                sbAudit.Append(" to ");
                sbAudit.Append(toAccount.AccountNumber);
                
                await _auditRepository.AddLogAsync(new AuditLog
                {
                    UserId = 1,
                    Action = "MoneyTransfer",
                    Details = sbAudit.ToString(),
                    IpAddress = "127.0.0.1"
                });

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Transfer hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Para yatırma işlemi yapar
        /// </summary>
        /// <param name="accountId">Hesap ID</param>
        /// <param name="amount">Yatırılacak tutar</param>
        /// <param name="description">Açıklama</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> DepositAsync(int accountId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                {
                    return "Yatırılacak tutar 0'dan büyük olmalıdır.";
                }

                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return "Hesap bulunamadı.";
                }

                account.Balance += amount;
                await _accountRepository.UpdateAsync(account);

                await _transactionRepository.AddAsync(new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Deposit",
                    Amount = amount,
                    Description = description ?? "Para yatırma"
                });

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Para yatırma hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }

        /// <summary>
        /// Para çekme işlemi yapar
        /// </summary>
        /// <param name="accountId">Hesap ID</param>
        /// <param name="amount">Çekilecek tutar</param>
        /// <param name="description">Açıklama</param>
        /// <returns>Başarılıysa null, hata varsa hata mesajı</returns>
        public async Task<string> WithdrawAsync(int accountId, decimal amount, string description)
        {
            try
            {
                if (amount <= 0)
                {
                    return "Çekilecek tutar 0'dan büyük olmalıdır.";
                }

                var account = await _accountRepository.GetByIdAsync(accountId);
                if (account == null)
                {
                    return "Hesap bulunamadı.";
                }

                if (account.Balance < amount)
                {
                    return "Yetersiz bakiye.";
                }

                account.Balance -= amount;
                await _accountRepository.UpdateAsync(account);

                await _transactionRepository.AddAsync(new Transaction
                {
                    AccountId = account.Id,
                    TransactionType = "Withdraw",
                    Amount = amount,
                    Description = description ?? "Para çekme"
                });

                return null; // Başarılı
            }
            catch (Exception ex)
            {
                var sbError = new StringBuilder();
                sbError.Append("Para çekme hatası: ");
                sbError.Append(ex.Message);
                return sbError.ToString();
            }
        }
    }
}

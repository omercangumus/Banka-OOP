using System;
using System.Threading.Tasks;
using BankApp.Core.Entities;
using BankApp.Core.Interfaces;

namespace BankApp.Infrastructure.Services
{
    public class TransactionService
    {
        private readonly IAccountRepository _accountRepository;
        private readonly ITransactionRepository _transactionRepository;
        private readonly IAuditRepository _auditRepository;

        public TransactionService(IAccountRepository accountRepo, ITransactionRepository transactionRepo, IAuditRepository auditRepo)
        {
            _accountRepository = accountRepo;
            _transactionRepository = transactionRepo;
            _auditRepository = auditRepo;
        }

        public async Task TransferMoneyAsync(int fromAccountId, string toIban, decimal amount, string description)
        {
            // Validations
            if (amount <= 0) throw new ArgumentException("Transfer tutarı 0'dan büyük olmalıdır.");

            var fromAccount = await _accountRepository.GetByIdAsync(fromAccountId);
            if (fromAccount == null) throw new Exception("Kaynak hesap bulunamadı.");

            var toAccount = await _accountRepository.GetByIBANAsync(toIban);
            if (toAccount == null) throw new Exception("Hedef IBAN bulunamadı.");

            if (fromAccount.Balance < amount) throw new Exception("Yetersiz bakiye.");

            // Debit Logic
            fromAccount.Balance -= amount;
            await _accountRepository.UpdateAsync(fromAccount);
            await _transactionRepository.AddAsync(new Transaction
            {
                AccountId = fromAccount.Id,
                TransactionType = "TransferOut",
                Amount = amount,
                Description = $"Transfer to {toIban}: {description}"
            });

            // Credit Logic
            toAccount.Balance += amount;
            await _accountRepository.UpdateAsync(toAccount);
            await _transactionRepository.AddAsync(new Transaction
            {
                AccountId = toAccount.Id,
                TransactionType = "TransferIn",
                Amount = amount,
                Description = $"Transfer from {fromAccount.IBAN}: {description}"
            });

            // Audit Log
            await _auditRepository.AddLogAsync(new AuditLog
            {
                UserId = 1, // Assumed Admin
                Action = "MoneyTransfer",
                Details = $"{amount} {fromAccount.CurrencyCode} from {fromAccount.AccountNumber} to {toAccount.AccountNumber}",
                IpAddress = "127.0.0.1" // Mock IP
            });
        }
    }
}

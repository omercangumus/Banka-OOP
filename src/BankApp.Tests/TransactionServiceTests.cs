using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using BankApp.Infrastructure.Services;
using BankApp.Core.Interfaces;
using BankApp.Core.Entities;

namespace BankApp.Tests
{
    public class TransactionServiceTests
    {
        private readonly Mock<IAccountRepository> _accountRepositoryMock;
        private readonly Mock<ITransactionRepository> _transactionRepositoryMock;
        private readonly Mock<IAuditRepository> _auditRepositoryMock;
        private readonly TransactionService _transactionService;

        public TransactionServiceTests()
        {
            _accountRepositoryMock = new Mock<IAccountRepository>();
            _transactionRepositoryMock = new Mock<ITransactionRepository>();
            _auditRepositoryMock = new Mock<IAuditRepository>();
            _transactionService = new TransactionService(_accountRepositoryMock.Object, _transactionRepositoryMock.Object, _auditRepositoryMock.Object);
        }

        [Fact]
        public async Task TransferMoneyAsync_ShouldTransferMoney_WhenBalanceIsSufficient()
        {
            // Arrange
            var fromAccountId = 1;
            var toIban = "TR123456";
            var amount = 100m;

            var fromAccount = new Account { Id = fromAccountId, Balance = 200m, IBAN = "TR111111", AccountNumber = "111", CurrencyCode = "TRY" };
            var toAccount = new Account { Id = 2, Balance = 50m, IBAN = toIban, AccountNumber = "222", CurrencyCode = "TRY" };

            _accountRepositoryMock.Setup(x => x.GetByIdAsync(fromAccountId)).ReturnsAsync(fromAccount);
            _accountRepositoryMock.Setup(x => x.GetByIBANAsync(toIban)).ReturnsAsync(toAccount);

            // Act
            await _transactionService.TransferMoneyAsync(fromAccountId, toIban, amount, "Test Transfer");

            // Assert
            fromAccount.Balance.Should().Be(100m);
            toAccount.Balance.Should().Be(150m);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(fromAccount), Times.Once);
            _accountRepositoryMock.Verify(x => x.UpdateAsync(toAccount), Times.Once);
            _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t => t.TransactionType == "TransferOut")), Times.Once);
            _transactionRepositoryMock.Verify(x => x.AddAsync(It.Is<Transaction>(t => t.TransactionType == "TransferIn")), Times.Once);
        }

        [Fact]
        public async Task TransferMoneyAsync_ShouldThrowException_WhenBalanceIsInsufficient()
        {
             // Arrange
            var fromAccountId = 1;
            var toIban = "TR123456";
            var amount = 300m; // More than balance

            var fromAccount = new Account { Id = fromAccountId, Balance = 200m };
            var toAccount = new Account { Id = 2, Balance = 50m, IBAN = toIban };

            _accountRepositoryMock.Setup(x => x.GetByIdAsync(fromAccountId)).ReturnsAsync(fromAccount);
            _accountRepositoryMock.Setup(x => x.GetByIBANAsync(toIban)).ReturnsAsync(toAccount);

            // Act
            Func<Task> act = async () => await _transactionService.TransferMoneyAsync(fromAccountId, toIban, amount, "Test");

            // Assert
            await act.Should().ThrowAsync<Exception>().WithMessage("Yetersiz bakiye.");
        }
    }
}

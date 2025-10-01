using System.Threading.Tasks;
using Xunit;
using Moq;
using FluentAssertions;
using BankApp.Infrastructure.Services;
using BankApp.Core.Interfaces;
using BankApp.Core.Entities;

namespace BankApp.Tests
{
    public class AuthServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IEmailService> _emailServiceMock;
        private readonly Mock<IAuditRepository> _auditRepositoryMock;
        private readonly AuthService _authService;

        public AuthServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _emailServiceMock = new Mock<IEmailService>();
            _auditRepositoryMock = new Mock<IAuditRepository>();
            _authService = new AuthService(_userRepositoryMock.Object, _emailServiceMock.Object, _auditRepositoryMock.Object);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnTrue_WhenCredentialsAreCorrect()
        {
            // Arrange
            var username = "testuser";
            var password = "password123";
            var hashedPassword = _authService.HashPassword(password);
            var user = new User { Id = 1, Username = username, PasswordHash = hashedPassword };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            result.Should().BeTrue();
            _auditRepositoryMock.Verify(x => x.AddLogAsync(It.Is<AuditLog>(l => l.Action == "LoginSuccess")), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenUserNotFound()
        {
            // Arrange
            var username = "nonexistent";
            var password = "password123";

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync((User)null);

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            result.Should().BeFalse();
            _auditRepositoryMock.Verify(x => x.AddLogAsync(It.Is<AuditLog>(l => l.Action == "LoginFailed")), Times.Once);
        }

        [Fact]
        public async Task LoginAsync_ShouldReturnFalse_WhenPasswordIsIncorrect()
        {
            // Arrange
            var username = "testuser";
            var password = "wrongpassword";
            var user = new User { Id = 1, Username = username, PasswordHash = "correcthash" };

            _userRepositoryMock.Setup(x => x.GetByUsernameAsync(username))
                .ReturnsAsync(user);

            // Act
            var result = await _authService.LoginAsync(username, password);

            // Assert
            result.Should().BeFalse();
            _auditRepositoryMock.Verify(x => x.AddLogAsync(It.Is<AuditLog>(l => l.Action == "LoginFailed")), Times.Once);
        }
    }
}

using FluentAssertions;
using Moq;
using TaskManagement.Application.Auth.Dtos;
using TaskManagement.Application.Auth.Services;
using TaskManagement.Application.Common.Exceptions;
using TaskManagement.Application.Common.Interfaces;
using TaskManagement.Application.Common.Security;
using TaskManagement.Application.Users.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.UnitTests.Auth;

public class AuthServiceTests
{
    private readonly Mock<IUserRepository> _users = new();
    private readonly Mock<IPasswordHasher> _passwordHasher = new();
    private readonly Mock<IJwtTokenGenerator> _tokenGenerator = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private AuthService CreateSut() =>
        new(_users.Object, _passwordHasher.Object, _tokenGenerator.Object, _currentUser.Object);

    // --- Register ---

    [Fact]
    public async Task RegisterAsync_hashes_password_persists_user_and_returns_token()
    {
        var sut = CreateSut();
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _passwordHasher.Setup(h => h.Hash("Passw0rd!")).Returns("HASHED");
        var expiry = DateTime.UtcNow.AddHours(1);
        _tokenGenerator.Setup(t => t.Generate(It.IsAny<User>())).Returns(new TokenResult("jwt-token", expiry));

        var result = await sut.RegisterAsync(new RegisterRequest("alice", "alice@example.com", "Passw0rd!"));

        result.AccessToken.Should().Be("jwt-token");
        result.ExpiresAtUtc.Should().Be(expiry);
        _passwordHasher.Verify(h => h.Hash("Passw0rd!"), Times.Once);
        _users.Verify(r => r.AddAsync(
            It.Is<User>(u => u.Username == "alice"
                          && u.Email == "alice@example.com"
                          && u.PasswordHash == "HASHED"),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_throws_Conflict_when_username_taken()
    {
        var sut = CreateSut();
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => sut.RegisterAsync(new RegisterRequest("alice", "alice@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_throws_Conflict_when_email_taken()
    {
        var sut = CreateSut();
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => sut.RegisterAsync(new RegisterRequest("alice", "alice@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}

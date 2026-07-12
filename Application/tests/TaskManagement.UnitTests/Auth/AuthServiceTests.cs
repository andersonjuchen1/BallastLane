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
    private readonly Mock<IUnitOfWork> _unitOfWork = new();

    private AuthService CreateSut() =>
        new(_users.Object, _passwordHasher.Object, _tokenGenerator.Object, _currentUser.Object, _unitOfWork.Object);

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
        _users.Verify(r => r.Add(
            It.Is<User>(u => u.Username == "alice"
                          && u.Email == "alice@example.com"
                          && u.PasswordHash == "HASHED")), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RegisterAsync_throws_Conflict_when_username_taken()
    {
        var sut = CreateSut();
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => sut.RegisterAsync(new RegisterRequest("alice", "alice@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task RegisterAsync_throws_Conflict_when_email_taken()
    {
        var sut = CreateSut();
        _users.Setup(r => r.ExistsByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _users.Setup(r => r.ExistsByEmailAsync("alice@example.com", It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Func<Task> act = () => sut.RegisterAsync(new RegisterRequest("alice", "alice@example.com", "Passw0rd!"));

        await act.Should().ThrowAsync<ConflictException>();
        _users.Verify(r => r.Add(It.IsAny<User>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    // --- Login ---

    [Fact]
    public async Task LoginAsync_returns_token_for_valid_credentials()
    {
        var sut = CreateSut();
        var user = new User("alice", "alice@example.com", "HASHED");
        _users.Setup(r => r.GetByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("HASHED", "Passw0rd!")).Returns(true);
        var expiry = DateTime.UtcNow.AddHours(1);
        _tokenGenerator.Setup(t => t.Generate(user)).Returns(new TokenResult("jwt-token", expiry));

        var result = await sut.LoginAsync(new LoginRequest("alice", "Passw0rd!"));

        result.AccessToken.Should().Be("jwt-token");
        result.ExpiresAtUtc.Should().Be(expiry);
    }

    [Fact]
    public async Task LoginAsync_throws_InvalidCredentials_when_user_not_found()
    {
        var sut = CreateSut();
        _users.Setup(r => r.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Func<Task> act = () => sut.LoginAsync(new LoginRequest("ghost", "whatever"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_throws_InvalidCredentials_when_password_is_wrong()
    {
        var sut = CreateSut();
        var user = new User("alice", "alice@example.com", "HASHED");
        _users.Setup(r => r.GetByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("HASHED", "wrong")).Returns(false);

        Func<Task> act = () => sut.LoginAsync(new LoginRequest("alice", "wrong"));

        await act.Should().ThrowAsync<InvalidCredentialsException>();
    }

    [Fact]
    public async Task LoginAsync_reports_the_same_generic_error_for_unknown_user_and_wrong_password()
    {
        var sut = CreateSut();
        _users.Setup(r => r.GetByUsernameAsync("ghost", It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);
        var user = new User("alice", "alice@example.com", "HASHED");
        _users.Setup(r => r.GetByUsernameAsync("alice", It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _passwordHasher.Setup(h => h.Verify("HASHED", "wrong")).Returns(false);

        var unknownUser = await Record.ExceptionAsync(() => sut.LoginAsync(new LoginRequest("ghost", "whatever")));
        var wrongPassword = await Record.ExceptionAsync(() => sut.LoginAsync(new LoginRequest("alice", "wrong")));

        unknownUser.Should().BeOfType<InvalidCredentialsException>();
        wrongPassword.Should().BeOfType<InvalidCredentialsException>();
        wrongPassword!.Message.Should().Be(unknownUser!.Message);
    }

    // --- Current user (/me) ---

    [Fact]
    public async Task GetCurrentUserAsync_returns_the_authenticated_user()
    {
        var sut = CreateSut();
        var user = new User("alice", "alice@example.com", "HASHED");
        _currentUser.SetupGet(x => x.UserId).Returns(user.Id);
        _users.Setup(r => r.GetByIdAsync(user.Id, It.IsAny<CancellationToken>())).ReturnsAsync(user);

        var result = await sut.GetCurrentUserAsync();

        result.Id.Should().Be(user.Id);
        result.Username.Should().Be("alice");
        result.Email.Should().Be("alice@example.com");
    }

    [Fact]
    public async Task GetCurrentUserAsync_throws_NotFound_when_user_no_longer_exists()
    {
        var sut = CreateSut();
        _currentUser.SetupGet(x => x.UserId).Returns(Guid.NewGuid());
        _users.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((User?)null);

        Func<Task> act = () => sut.GetCurrentUserAsync();

        await act.Should().ThrowAsync<NotFoundException>();
    }
}

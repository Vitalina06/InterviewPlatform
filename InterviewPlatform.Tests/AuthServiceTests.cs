using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;
using InterviewPlatform.Api.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InterviewPlatform.Tests;

public class AuthServiceTests
{
    private const string Password = "Hr123456";
    private readonly BcryptPasswordHasher _hasher = new();

    private User CreateUser()
    {
        return new User
        {
            Id = 1,
            FullName = "Иванова Анна",
            Login = "hr1",
            PasswordHash = _hasher.Hash(Password),
            Roles = new List<string> { AppRoles.Hr }
        };
    }

    private AuthService CreateService(User? user)
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByLoginAsync("hr1")).ReturnsAsync(user);

        var tokens = new Mock<ITokenService>();
        tokens.Setup(t => t.CreateToken(It.IsAny<User>())).Returns("test-token");

        var settings = new Dictionary<string, string?>
        {
            ["Auth:MaxFailedAttempts"] = "5",
            ["Auth:LockoutMinutes"] = "15"
        };
        var config = new ConfigurationBuilder().AddInMemoryCollection(settings).Build();

        return new AuthService(repo.Object, _hasher, tokens.Object, config, NullLogger<AuthService>.Instance);
    }

    [Fact]
    public async Task Login_UnknownUser_Fails()
    {
        var service = CreateService(null);

        var result = await service.LoginAsync(new LoginRequest { Login = "hr1", Password = Password });

        Assert.False(result.Success);
        Assert.False(result.IsLocked);
    }

    [Fact]
    public async Task Login_WrongPassword_IncreasesFailedCount()
    {
        var user = CreateUser();
        var service = CreateService(user);

        var result = await service.LoginAsync(new LoginRequest { Login = "hr1", Password = "wrong" });

        Assert.False(result.Success);
        Assert.Equal(1, user.FailedLoginCount);
    }

    [Fact]
    public async Task Login_FiveWrongAttempts_LocksUserEvenWithCorrectPassword()
    {
        var user = CreateUser();
        var service = CreateService(user);

        for (var i = 0; i < 5; i++)
        {
            await service.LoginAsync(new LoginRequest { Login = "hr1", Password = "wrong" });
        }

        Assert.NotNull(user.LockoutEnd);

        var result = await service.LoginAsync(new LoginRequest { Login = "hr1", Password = Password });

        Assert.True(result.IsLocked);
        Assert.False(result.Success);
    }

    [Fact]
    public async Task Login_CorrectPassword_ReturnsTokenAndResetsCounter()
    {
        var user = CreateUser();
        user.FailedLoginCount = 3;
        var service = CreateService(user);

        var result = await service.LoginAsync(new LoginRequest { Login = "hr1", Password = Password });

        Assert.True(result.Success);
        Assert.Equal("test-token", result.Token);
        Assert.Equal(0, user.FailedLoginCount);
    }
}

using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;
using InterviewPlatform.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InterviewPlatform.Tests;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _repo = new();
    private readonly BcryptPasswordHasher _hasher = new();

    private UserService CreateService()
    {
        return new UserService(_repo.Object, _hasher, NullLogger<UserService>.Instance);
    }

    private static CreateUserRequest CreateRequest(string role)
    {
        return new CreateUserRequest
        {
            FullName = "Иванова Анна",
            Login = "hr1",
            Password = "Hr123456",
            Roles = new List<string> { role }
        };
    }

    [Fact]
    public async Task Create_UnknownRole_ReturnsError()
    {
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest("SuperAdmin"));

        Assert.Null(result.User);
        Assert.Contains("Неизвестная роль", result.Error);
    }

    [Fact]
    public async Task Create_DuplicateLogin_ReturnsError()
    {
        _repo.Setup(r => r.GetByLoginAsync("hr1")).ReturnsAsync(new User { Login = "hr1" });
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest(AppRoles.Hr));

        Assert.Null(result.User);
        Assert.Contains("уже существует", result.Error);
    }

    [Fact]
    public async Task Create_ValidUser_StoresHashInsteadOfPassword()
    {
        User? saved = null;
        _repo.Setup(r => r.AddAsync(It.IsAny<User>()))
            .Callback<User>(u => saved = u)
            .Returns(Task.CompletedTask);
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest(AppRoles.Hr));

        Assert.Null(result.Error);
        Assert.NotNull(saved);
        Assert.NotEqual("Hr123456", saved!.PasswordHash);
        Assert.True(_hasher.Verify("Hr123456", saved.PasswordHash));
    }
}

using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ILogger<UserService> _logger;

    public UserService(IUserRepository users, IPasswordHasher hasher, ILogger<UserService> logger)
    {
        _users = users;
        _hasher = hasher;
        _logger = logger;
    }

    public async Task<(UserDto? User, string? Error)> CreateAsync(CreateUserRequest request)
    {
        foreach (var role in request.Roles)
        {
            if (!AppRoles.All.Contains(role))
            {
                return (null, $"Неизвестная роль: {role}");
            }
        }

        var existing = await _users.GetByLoginAsync(request.Login);
        if (existing != null)
        {
            return (null, "Пользователь с таким логином уже существует");
        }

        var user = new User
        {
            FullName = request.FullName,
            Login = request.Login,
            PasswordHash = _hasher.Hash(request.Password),
            Roles = request.Roles.Distinct().ToList()
        };

        await _users.AddAsync(user);
        _logger.LogInformation("Создан пользователь {Login}", user.Login);

        return (ToDto(user), null);
    }

    public async Task<List<UserDto>> GetAllAsync()
    {
        var users = await _users.GetAllAsync();
        return users.Select(ToDto).ToList();
    }

    private static UserDto ToDto(User user)
    {
        return new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Login = user.Login,
            Roles = user.Roles
        };
    }
}

using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _hasher;
    private readonly ITokenService _tokens;
    private readonly ILogger<AuthService> _logger;
    private readonly int _maxAttempts;
    private readonly int _lockoutMinutes;

    public AuthService(
        IUserRepository users,
        IPasswordHasher hasher,
        ITokenService tokens,
        IConfiguration config,
        ILogger<AuthService> logger)
    {
        _users = users;
        _hasher = hasher;
        _tokens = tokens;
        _logger = logger;
        _maxAttempts = config.GetValue<int>("Auth:MaxFailedAttempts");
        _lockoutMinutes = config.GetValue<int>("Auth:LockoutMinutes");
    }

    public async Task<LoginResult> LoginAsync(LoginRequest request)
    {
        var user = await _users.GetByLoginAsync(request.Login);

        if (user == null)
        {
            _logger.LogWarning("Попытка входа с несуществующим логином {Login}", request.Login);
            return new LoginResult { Error = "Неверный логин или пароль" };
        }

        // проверяем, не заблокирован ли вход
        if (user.LockoutEnd != null && user.LockoutEnd > DateTime.UtcNow)
        {
            var left = (int)Math.Ceiling((user.LockoutEnd.Value - DateTime.UtcNow).TotalMinutes);
            _logger.LogWarning("Вход заблокирован для пользователя {Login}", user.Login);
            return new LoginResult { IsLocked = true, Error = $"Вход заблокирован. Повторите через {left} мин." };
        }

        if (!_hasher.Verify(request.Password, user.PasswordHash))
        {
            user.FailedLoginCount++;

            if (user.FailedLoginCount >= _maxAttempts)
            {
                user.LockoutEnd = DateTime.UtcNow.AddMinutes(_lockoutMinutes);
                user.FailedLoginCount = 0;
                _logger.LogWarning("Пользователь {Login} заблокирован на {Minutes} мин.", user.Login, _lockoutMinutes);
            }

            await _users.SaveAsync();
            return new LoginResult { Error = "Неверный логин или пароль" };
        }

        // успешный вход - сбрасываем счётчик
        user.FailedLoginCount = 0;
        user.LockoutEnd = null;
        await _users.SaveAsync();

        _logger.LogInformation("Пользователь {Login} вошёл в систему", user.Login);
        return new LoginResult { Success = true, Token = _tokens.CreateToken(user) };
    }
}

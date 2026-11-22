using InterviewPlatform.Api.DTOs;

namespace InterviewPlatform.Api.Services;

public interface IUserService
{
    Task<(UserDto? User, string? Error)> CreateAsync(CreateUserRequest request);
    Task<List<UserDto>> GetAllAsync();
}

using InterviewPlatform.Api.DTOs;

namespace InterviewPlatform.Api.Services;

public interface IAuthService
{
    Task<LoginResult> LoginAsync(LoginRequest request);
}

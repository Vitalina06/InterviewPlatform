using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Services;

public interface ITokenService
{
    string CreateToken(User user);
}

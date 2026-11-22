using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _auth.LoginAsync(request);

        if (result.IsLocked)
        {
            return StatusCode(StatusCodes.Status423Locked, new { error = result.Error });
        }

        if (!result.Success)
        {
            return Unauthorized(new { error = result.Error });
        }

        return Ok(new { token = result.Token });
    }
}

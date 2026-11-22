using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = AppRoles.Admin)]
public class UsersController : ControllerBase
{
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _users.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateUserRequest request)
    {
        var (user, error) = await _users.CreateAsync(request);

        if (error != null)
        {
            return BadRequest(new { error });
        }

        return Ok(user);
    }
}

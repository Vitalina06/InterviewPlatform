using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/candidates")]
public class CandidatesController : ControllerBase
{
    private readonly ICandidateService _candidates;

    public CandidatesController(ICandidateService candidates)
    {
        _candidates = candidates;
    }

    // смотреть реестр могут HR и согласующий
    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Hr},{AppRoles.Approver}")]
    public async Task<IActionResult> Search([FromQuery] string? search)
    {
        return Ok(await _candidates.SearchAsync(search));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{AppRoles.Hr},{AppRoles.Approver}")]
    public async Task<IActionResult> GetById(int id)
    {
        var candidate = await _candidates.GetByIdAsync(id);

        if (candidate == null)
        {
            return NotFound();
        }

        return Ok(candidate);
    }

    // создавать - только HR
    [HttpPost]
    [Authorize(Roles = AppRoles.Hr)]
    public async Task<IActionResult> Create(CreateCandidateRequest request)
    {
        var candidate = await _candidates.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = candidate.Id }, candidate);
    }
}

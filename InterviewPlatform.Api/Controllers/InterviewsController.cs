using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/interviews")]
public class InterviewsController : ControllerBase
{
    private readonly IInterviewService _interviews;

    public InterviewsController(IInterviewService interviews)
    {
        _interviews = interviews;
    }

    [HttpGet]
    [Authorize(Roles = $"{AppRoles.Hr},{AppRoles.Approver}")]
    public async Task<IActionResult> GetList([FromQuery] int? candidateId)
    {
        return Ok(await _interviews.GetListAsync(candidateId));
    }

    [HttpGet("{id}")]
    [Authorize(Roles = $"{AppRoles.Hr},{AppRoles.Approver}")]
    public async Task<IActionResult> GetById(int id)
    {
        var interview = await _interviews.GetByIdAsync(id);

        if (interview == null)
        {
            return NotFound();
        }

        return Ok(interview);
    }

    [HttpPost]
    [Authorize(Roles = AppRoles.Hr)]
    public async Task<IActionResult> Create(CreateInterviewRequest request)
    {
        var (interview, error) = await _interviews.CreateAsync(request);

        if (error != null)
        {
            return BadRequest(new { error });
        }

        return CreatedAtAction(nameof(GetById), new { id = interview!.Id }, interview);
    }

    [HttpPut("{id}/ratings")]
    [Authorize(Roles = AppRoles.Hr)]
    public async Task<IActionResult> UpdateRatings(int id, UpdateRatingsRequest request)
    {
        var (interview, error) = await _interviews.UpdateRatingsAsync(id, request);

        if (error != null)
        {
            return BadRequest(new { error });
        }

        return Ok(interview);
    }

    [HttpPut("{id}/decision")]
    [Authorize(Roles = AppRoles.Approver)]
    public async Task<IActionResult> SetDecision(int id, DecisionRequest request)
    {
        var (interview, error) = await _interviews.SetDecisionAsync(id, request.Status);

        if (error != null)
        {
            return BadRequest(new { error });
        }

        return Ok(interview);
    }
}

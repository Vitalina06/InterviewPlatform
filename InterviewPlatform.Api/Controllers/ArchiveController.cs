using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/archive")]
[Authorize(Roles = AppRoles.Hr)]
public class ArchiveController : ControllerBase
{
    private readonly IArchiveService _archive;

    public ArchiveController(IArchiveService archive)
    {
        _archive = archive;
    }

    [HttpGet("candidates")]
    public async Task<IActionResult> GetCandidates()
    {
        return Ok(await _archive.GetArchivedCandidatesAsync());
    }

    [HttpGet("interviews")]
    public async Task<IActionResult> GetInterviews()
    {
        return Ok(await _archive.GetArchivedInterviewsAsync());
    }

    [HttpPost("candidates/{id}")]
    public async Task<IActionResult> ArchiveCandidate(int id)
    {
        return await Result(await _archive.SetCandidateArchivedAsync(id, true));
    }

    [HttpPost("candidates/{id}/restore")]
    public async Task<IActionResult> RestoreCandidate(int id)
    {
        return await Result(await _archive.SetCandidateArchivedAsync(id, false));
    }

    [HttpPost("interviews/{id}")]
    public async Task<IActionResult> ArchiveInterview(int id)
    {
        return await Result(await _archive.SetInterviewArchivedAsync(id, true));
    }

    [HttpPost("interviews/{id}/restore")]
    public async Task<IActionResult> RestoreInterview(int id)
    {
        return await Result(await _archive.SetInterviewArchivedAsync(id, false));
    }

    private static Task<IActionResult> Result(bool found)
    {
        IActionResult result = found ? new NoContentResult() : new NotFoundResult();
        return Task.FromResult(result);
    }
}

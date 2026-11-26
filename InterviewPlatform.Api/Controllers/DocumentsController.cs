using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using InterviewPlatform.Shared;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/documents")]
[Authorize(Roles = AppRoles.Hr)]
public class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;

    public DocumentsController(IDocumentService documents)
    {
        _documents = documents;
    }

    [HttpPost("interview-protocol/{interviewId}")]
    public async Task<IActionResult> Protocol(int interviewId)
    {
        return await RequestInterviewDocument(interviewId, DocumentType.InterviewProtocol);
    }

    [HttpPost("invitation/{interviewId}")]
    public async Task<IActionResult> Invitation(int interviewId)
    {
        return await RequestInterviewDocument(interviewId, DocumentType.Invitation);
    }

    [HttpPost("rejection/{interviewId}")]
    public async Task<IActionResult> Rejection(int interviewId)
    {
        return await RequestInterviewDocument(interviewId, DocumentType.Rejection);
    }

    [HttpPost("candidate-card/{candidateId}")]
    public async Task<IActionResult> CandidateCard(int candidateId)
    {
        var (result, error) = await _documents.RequestCandidateCardAsync(candidateId);

        if (error != null)
        {
            return NotFound(new { error });
        }

        return Accepted(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetInfo(string id)
    {
        var info = await _documents.GetInfoAsync(id);

        if (info == null)
        {
            return NotFound();
        }

        return Ok(info);
    }

    [HttpGet("{id}/file")]
    public async Task<IActionResult> GetFile(string id)
    {
        var record = await _documents.GetFileAsync(id);

        if (record == null)
        {
            return NotFound();
        }

        if (record.Status != DocumentStatus.Ready || record.Content == null)
        {
            return Conflict(new { error = "Документ ещё не готов", status = record.Status.ToString() });
        }

        return File(record.Content, "application/pdf", record.FileName);
    }

    private async Task<IActionResult> RequestInterviewDocument(int interviewId, DocumentType type)
    {
        var (result, error) = await _documents.RequestInterviewDocumentAsync(interviewId, type);

        if (error != null)
        {
            return NotFound(new { error });
        }

        return Accepted(result);
    }
}

using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Shared;

namespace InterviewPlatform.Api.Services;

public interface IDocumentService
{
    Task<(DocumentRequestDto? Result, string? Error)> RequestInterviewDocumentAsync(int interviewId, DocumentType type);
    Task<(DocumentRequestDto? Result, string? Error)> RequestCandidateCardAsync(int candidateId);
    Task<DocumentInfoDto?> GetInfoAsync(string id);
    Task<DocumentRecord?> GetFileAsync(string id);
}

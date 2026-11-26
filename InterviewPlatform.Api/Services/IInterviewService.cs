using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Services;

public interface IInterviewService
{
    Task<(InterviewDto? Interview, string? Error)> CreateAsync(CreateInterviewRequest request);
    Task<(InterviewDto? Interview, string? Error)> UpdateRatingsAsync(int id, UpdateRatingsRequest request);
    Task<(InterviewDto? Interview, string? Error)> SetDecisionAsync(int id, InterviewStatus status);
    Task<InterviewDto?> GetByIdAsync(int id);
    Task<List<InterviewDto>> GetListAsync(int? candidateId, bool archived = false);
}

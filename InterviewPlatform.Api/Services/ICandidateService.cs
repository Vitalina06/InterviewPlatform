using InterviewPlatform.Api.DTOs;

namespace InterviewPlatform.Api.Services;

public interface ICandidateService
{
    Task<CandidateDto> CreateAsync(CreateCandidateRequest request);
    Task<CandidateDto?> GetByIdAsync(int id);
    Task<List<CandidateDto>> SearchAsync(string? search);
}

using InterviewPlatform.Api.DTOs;

namespace InterviewPlatform.Api.Services;

public interface IArchiveService
{
    Task<bool> SetCandidateArchivedAsync(int id, bool archived);
    Task<bool> SetInterviewArchivedAsync(int id, bool archived);
    Task<List<CandidateDto>> GetArchivedCandidatesAsync();
    Task<List<InterviewDto>> GetArchivedInterviewsAsync();
}

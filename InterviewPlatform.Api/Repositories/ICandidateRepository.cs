using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Repositories;

public interface ICandidateRepository
{
    Task AddAsync(Candidate candidate);
    Task<Candidate?> GetByIdAsync(int id);
    Task<List<Candidate>> SearchAsync(string? search, bool archived);
    Task<bool> SetArchivedAsync(int id, bool archived);
}

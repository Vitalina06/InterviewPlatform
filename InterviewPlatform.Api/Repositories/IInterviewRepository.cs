using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Repositories;

public interface IInterviewRepository
{
    Task AddAsync(Interview interview);
    Task<Interview?> GetByIdAsync(int id);
    Task<List<Interview>> GetListAsync(int? candidateId, bool archived);
    Task<bool> SetArchivedAsync(int id, bool archived);
    Task SaveAsync();
}

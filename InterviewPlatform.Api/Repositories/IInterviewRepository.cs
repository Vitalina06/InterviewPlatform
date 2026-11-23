using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Repositories;

public interface IInterviewRepository
{
    Task AddAsync(Interview interview);
    Task<Interview?> GetByIdAsync(int id);
    Task<List<Interview>> GetListAsync(int? candidateId);
    Task SaveAsync();
}

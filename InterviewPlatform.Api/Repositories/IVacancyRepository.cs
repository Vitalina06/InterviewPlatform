using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Repositories;

public interface IVacancyRepository
{
    Task<List<Competency>> GetCompetenciesByNamesAsync(List<string> names);
    Task AddAsync(Vacancy vacancy);
    Task<List<Vacancy>> GetAllAsync();
    Task<Vacancy?> GetByIdAsync(int id);
}

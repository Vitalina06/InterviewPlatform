using InterviewPlatform.Api.DTOs;

namespace InterviewPlatform.Api.Services;

public interface IVacancyService
{
    Task<VacancyDto> CreateAsync(CreateVacancyRequest request);
    Task<List<VacancyDto>> GetAllAsync();
}

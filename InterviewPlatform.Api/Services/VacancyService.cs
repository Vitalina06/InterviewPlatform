using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class VacancyService : IVacancyService
{
    private readonly IVacancyRepository _vacancies;
    private readonly ILogger<VacancyService> _logger;

    public VacancyService(IVacancyRepository vacancies, ILogger<VacancyService> logger)
    {
        _vacancies = vacancies;
        _logger = logger;
    }

    public async Task<VacancyDto> CreateAsync(CreateVacancyRequest request)
    {
        var names = request.Competencies
            .Select(n => n.Trim())
            .Where(n => n != "")
            .Distinct()
            .ToList();

        // уже существующие компетенции переиспользуем, новые создаём
        var competencies = await _vacancies.GetCompetenciesByNamesAsync(names);

        foreach (var name in names)
        {
            if (!competencies.Any(c => c.Name == name))
            {
                competencies.Add(new Competency { Name = name });
            }
        }

        var vacancy = new Vacancy
        {
            Title = request.Title.Trim(),
            Competencies = competencies
        };

        await _vacancies.AddAsync(vacancy);
        _logger.LogInformation("Создана вакансия {VacancyId}", vacancy.Id);

        return ToDto(vacancy);
    }

    public async Task<List<VacancyDto>> GetAllAsync()
    {
        var vacancies = await _vacancies.GetAllAsync();
        return vacancies.Select(ToDto).ToList();
    }

    private static VacancyDto ToDto(Vacancy vacancy)
    {
        return new VacancyDto
        {
            Id = vacancy.Id,
            Title = vacancy.Title,
            Competencies = vacancy.Competencies
                .OrderBy(c => c.Id)
                .Select(c => new CompetencyDto { Id = c.Id, Name = c.Name })
                .ToList()
        };
    }
}

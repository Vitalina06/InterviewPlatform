using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class InterviewService : IInterviewService
{
    private readonly IInterviewRepository _interviews;
    private readonly ICandidateRepository _candidates;
    private readonly IVacancyRepository _vacancies;
    private readonly ILogger<InterviewService> _logger;

    public InterviewService(
        IInterviewRepository interviews,
        ICandidateRepository candidates,
        IVacancyRepository vacancies,
        ILogger<InterviewService> logger)
    {
        _interviews = interviews;
        _candidates = candidates;
        _vacancies = vacancies;
        _logger = logger;
    }

    public async Task<(InterviewDto? Interview, string? Error)> CreateAsync(CreateInterviewRequest request)
    {
        var candidate = await _candidates.GetByIdAsync(request.CandidateId);
        if (candidate == null)
        {
            return (null, "Кандидат не найден");
        }

        if (candidate.IsArchived)
        {
            return (null, "Кандидат в архиве, сначала восстановите его");
        }

        var vacancy = await _vacancies.GetByIdAsync(request.VacancyId);
        if (vacancy == null)
        {
            return (null, "Вакансия не найдена");
        }

        var (ratings, error) = BuildRatings(vacancy, request.Ratings);
        if (error != null)
        {
            return (null, error);
        }

        var interview = new Interview
        {
            CandidateId = candidate.Id,
            VacancyId = vacancy.Id,
            ScheduledAt = request.ScheduledAt.ToUniversalTime(),
            Comment = request.Comment?.Trim() ?? "",
            Status = InterviewStatus.Pending,
            Stages = request.Stages.Select(s => new InterviewStage { Name = s.Trim() }).ToList(),
            Ratings = ratings
        };

        await _interviews.AddAsync(interview);
        _logger.LogInformation("Создано собеседование {InterviewId} для кандидата {CandidateId}", interview.Id, candidate.Id);

        // перечитываем, чтобы в ответе были имя кандидата и название вакансии
        var created = await _interviews.GetByIdAsync(interview.Id);
        return (ToDto(created!), null);
    }

    public async Task<(InterviewDto? Interview, string? Error)> UpdateRatingsAsync(int id, UpdateRatingsRequest request)
    {
        var interview = await _interviews.GetByIdAsync(id);
        if (interview == null)
        {
            return (null, "Собеседование не найдено");
        }

        var vacancy = await _vacancies.GetByIdAsync(interview.VacancyId);

        var (ratings, error) = BuildRatings(vacancy!, request.Ratings);
        if (error != null)
        {
            return (null, error);
        }

        // старую матрицу заменяем новой
        interview.Ratings.Clear();
        foreach (var rating in ratings)
        {
            interview.Ratings.Add(rating);
        }

        if (request.Comment != null)
        {
            interview.Comment = request.Comment.Trim();
        }

        await _interviews.SaveAsync();
        _logger.LogInformation("Обновлены оценки собеседования {InterviewId}", id);

        return (ToDto(interview), null);
    }

    public async Task<(InterviewDto? Interview, string? Error)> SetDecisionAsync(int id, InterviewStatus status)
    {
        if (status == InterviewStatus.Pending)
        {
            return (null, "Нужно выбрать: Accepted, Rejected или NextStage");
        }

        var interview = await _interviews.GetByIdAsync(id);
        if (interview == null)
        {
            return (null, "Собеседование не найдено");
        }

        interview.Status = status;
        await _interviews.SaveAsync();
        _logger.LogInformation("Решение по собеседованию {InterviewId}: {Status}", id, status);

        return (ToDto(interview), null);
    }

    public async Task<InterviewDto?> GetByIdAsync(int id)
    {
        var interview = await _interviews.GetByIdAsync(id);
        return interview == null ? null : ToDto(interview);
    }

    public async Task<List<InterviewDto>> GetListAsync(int? candidateId, bool archived = false)
    {
        var interviews = await _interviews.GetListAsync(candidateId, archived);
        return interviews.Select(ToDto).ToList();
    }

    // проверяет оценки: компетенция должна относиться к вакансии и не повторяться
    private static (List<CompetencyRating> Ratings, string? Error) BuildRatings(Vacancy vacancy, List<RatingInput> inputs)
    {
        var ratings = new List<CompetencyRating>();

        foreach (var input in inputs)
        {
            var competency = vacancy.Competencies.FirstOrDefault(c => c.Id == input.CompetencyId);

            if (competency == null)
            {
                return (ratings, $"Компетенция {input.CompetencyId} не относится к этой вакансии");
            }

            if (ratings.Any(r => r.CompetencyId == input.CompetencyId))
            {
                return (ratings, $"Компетенция {input.CompetencyId} указана дважды");
            }

            ratings.Add(new CompetencyRating
            {
                CompetencyId = competency.Id,
                Competency = competency,
                Score = input.Score,
                Comment = input.Comment?.Trim() ?? ""
            });
        }

        return (ratings, null);
    }

    private static InterviewDto ToDto(Interview interview)
    {
        return new InterviewDto
        {
            Id = interview.Id,
            CandidateId = interview.CandidateId,
            CandidateName = interview.Candidate?.FullName ?? "",
            VacancyId = interview.VacancyId,
            VacancyTitle = interview.Vacancy?.Title ?? "",
            ScheduledAt = interview.ScheduledAt,
            Comment = interview.Comment,
            Status = interview.Status,
            Stages = interview.Stages.OrderBy(s => s.Id).Select(s => s.Name).ToList(),
            Ratings = interview.Ratings
                .OrderBy(r => r.CompetencyId)
                .Select(r => new RatingDto
                {
                    CompetencyId = r.CompetencyId,
                    CompetencyName = r.Competency?.Name ?? "",
                    Score = r.Score,
                    Comment = r.Comment
                })
                .ToList()
        };
    }
}

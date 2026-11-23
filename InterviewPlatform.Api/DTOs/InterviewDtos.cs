using System.ComponentModel.DataAnnotations;
using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.DTOs;

public class RatingInput
{
    [Range(1, int.MaxValue)]
    public int CompetencyId { get; set; }

    [Range(1, 5)]
    public int Score { get; set; }

    public string? Comment { get; set; }
}

public class CreateInterviewRequest
{
    [Range(1, int.MaxValue)]
    public int CandidateId { get; set; }

    [Range(1, int.MaxValue)]
    public int VacancyId { get; set; }

    public DateTime ScheduledAt { get; set; }

    [Required, MinLength(1, ErrorMessage = "Нужен минимум один этап")]
    public List<string> Stages { get; set; } = new();

    public string? Comment { get; set; }

    public List<RatingInput> Ratings { get; set; } = new();
}

public class UpdateRatingsRequest
{
    public string? Comment { get; set; }
    public List<RatingInput> Ratings { get; set; } = new();
}

public class DecisionRequest
{
    public InterviewStatus Status { get; set; }
}

public class RatingDto
{
    public int CompetencyId { get; set; }
    public string CompetencyName { get; set; } = "";
    public int Score { get; set; }
    public string Comment { get; set; } = "";
}

public class InterviewDto
{
    public int Id { get; set; }
    public int CandidateId { get; set; }
    public string CandidateName { get; set; } = "";
    public int VacancyId { get; set; }
    public string VacancyTitle { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public string Comment { get; set; } = "";
    public InterviewStatus Status { get; set; }
    public List<string> Stages { get; set; } = new();
    public List<RatingDto> Ratings { get; set; } = new();
}

// краткая информация для реестра кандидатов
public class InterviewShortDto
{
    public int Id { get; set; }
    public string VacancyTitle { get; set; } = "";
    public DateTime ScheduledAt { get; set; }
    public InterviewStatus Status { get; set; }
}

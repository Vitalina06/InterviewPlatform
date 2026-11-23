using System.ComponentModel.DataAnnotations;

namespace InterviewPlatform.Api.DTOs;

public class CreateVacancyRequest
{
    [Required]
    public string Title { get; set; } = "";

    [Required, MinLength(1, ErrorMessage = "Нужна минимум одна компетенция")]
    public List<string> Competencies { get; set; } = new();
}

public class CompetencyDto
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
}

public class VacancyDto
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<CompetencyDto> Competencies { get; set; } = new();
}

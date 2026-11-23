using System.ComponentModel.DataAnnotations;

namespace InterviewPlatform.Api.DTOs;

public class SkillDto
{
    [Required]
    public string Name { get; set; } = "";

    [Range(1, 5)]
    public int Rating { get; set; }
}

public class CreateCandidateRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required]
    [RegularExpression(@"^\+?[0-9\s\-()]{7,20}$", ErrorMessage = "Некорректный номер телефона")]
    public string Phone { get; set; } = "";

    [Required]
    public string City { get; set; } = "";

    [Required, MinLength(1, ErrorMessage = "Нужен минимум один навык")]
    public List<SkillDto> Skills { get; set; } = new();
}

public class CandidateDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public List<SkillDto> Skills { get; set; } = new();
    public List<InterviewShortDto> Interviews { get; set; } = new();
}

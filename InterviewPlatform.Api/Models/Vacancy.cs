namespace InterviewPlatform.Api.Models;

public class Vacancy
{
    public int Id { get; set; }
    public string Title { get; set; } = "";
    public List<Competency> Competencies { get; set; } = new();
}

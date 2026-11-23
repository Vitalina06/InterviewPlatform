namespace InterviewPlatform.Api.Models;

public class Competency
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public List<Vacancy> Vacancies { get; set; } = new();
}

namespace InterviewPlatform.Api.Models;

public class CompetencyRating
{
    public int Id { get; set; }
    public int InterviewId { get; set; }

    public int CompetencyId { get; set; }
    public Competency? Competency { get; set; }

    public int Score { get; set; }
    public string Comment { get; set; } = "";
}

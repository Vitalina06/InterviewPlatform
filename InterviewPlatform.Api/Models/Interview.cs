namespace InterviewPlatform.Api.Models;

public class Interview
{
    public int Id { get; set; }

    public int CandidateId { get; set; }
    public Candidate? Candidate { get; set; }

    public int VacancyId { get; set; }
    public Vacancy? Vacancy { get; set; }

    public DateTime ScheduledAt { get; set; }
    public bool IsArchived { get; set; }
    public string Comment { get; set; } = "";
    public InterviewStatus Status { get; set; } = InterviewStatus.Pending;

    public List<InterviewStage> Stages { get; set; } = new();
    public List<CompetencyRating> Ratings { get; set; } = new();
}

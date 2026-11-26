namespace InterviewPlatform.Api.Models;

public class Candidate
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public bool IsArchived { get; set; }
    public List<Skill> Skills { get; set; } = new();
    public List<Interview> Interviews { get; set; } = new();
}

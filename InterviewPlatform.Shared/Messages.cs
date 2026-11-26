namespace InterviewPlatform.Shared;

public enum DocumentType
{
    InterviewProtocol,
    Invitation,
    Rejection,
    CandidateCard
}

public enum DocumentStatus
{
    Pending,
    Ready,
    Failed
}

public class SkillData
{
    public string Name { get; set; } = "";
    public int Rating { get; set; }
}

public class RatingData
{
    public string Competency { get; set; } = "";
    public int Score { get; set; }
    public string Comment { get; set; } = "";
}

// все данные, которые нужны для PDF
public class DocumentData
{
    public string CandidateName { get; set; } = "";
    public string Phone { get; set; } = "";
    public string City { get; set; } = "";
    public List<SkillData> Skills { get; set; } = new();
    public string VacancyTitle { get; set; } = "";
    public DateTime? ScheduledAt { get; set; }
    public string InterviewComment { get; set; } = "";
    public string Status { get; set; } = "";
    public List<string> Stages { get; set; } = new();
    public List<RatingData> Ratings { get; set; } = new();
    public List<string> Interviews { get; set; } = new();
}

// сообщение в топик pdf-requests
public class PdfRequestMessage
{
    public string RequestId { get; set; } = "";
    public DocumentType Type { get; set; }
    public DocumentData Data { get; set; } = new();
}

// сообщение в топик audit-events
public class AuditMessage
{
    public string Action { get; set; } = "";
    public string UserLogin { get; set; } = "";
    public string Details { get; set; } = "";
    public DateTime Time { get; set; }
}

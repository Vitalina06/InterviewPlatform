namespace InterviewPlatform.Api.Services;

public interface IAuditService
{
    Task LogAsync(string action, string details);
}

using InterviewPlatform.Shared;

namespace InterviewPlatform.Api.DTOs;

public class DocumentRequestDto
{
    public string RequestId { get; set; } = "";
    public DocumentStatus Status { get; set; }
}

public class DocumentInfoDto
{
    public string Id { get; set; } = "";
    public DocumentType Type { get; set; }
    public string FileName { get; set; } = "";
    public DocumentStatus Status { get; set; }
    public string? Error { get; set; }
}

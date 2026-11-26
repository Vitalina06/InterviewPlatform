using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Shared;

namespace InterviewPlatform.Api.Services;

public class DocumentService : IDocumentService
{
    private readonly IInterviewService _interviews;
    private readonly ICandidateService _candidates;
    private readonly IDocumentStore _store;
    private readonly IMessagePublisher _publisher;
    private readonly IAuditService _audit;
    private readonly ILogger<DocumentService> _logger;
    private readonly string _pdfTopic;

    public DocumentService(
        IInterviewService interviews,
        ICandidateService candidates,
        IDocumentStore store,
        IMessagePublisher publisher,
        IAuditService audit,
        IConfiguration config,
        ILogger<DocumentService> logger)
    {
        _interviews = interviews;
        _candidates = candidates;
        _store = store;
        _publisher = publisher;
        _audit = audit;
        _logger = logger;
        _pdfTopic = config["Kafka:PdfTopic"]!;
    }

    public async Task<(DocumentRequestDto? Result, string? Error)> RequestInterviewDocumentAsync(int interviewId, DocumentType type)
    {
        var interview = await _interviews.GetByIdAsync(interviewId);
        if (interview == null)
        {
            return (null, "Собеседование не найдено");
        }

        var data = new DocumentData
        {
            CandidateName = interview.CandidateName,
            VacancyTitle = interview.VacancyTitle,
            ScheduledAt = interview.ScheduledAt,
            InterviewComment = interview.Comment,
            Status = interview.Status.ToString(),
            Stages = interview.Stages,
            Ratings = interview.Ratings
                .Select(r => new RatingData
                {
                    Competency = r.CompetencyName,
                    Score = r.Score,
                    Comment = r.Comment
                })
                .ToList()
        };

        var fileName = $"{type}_{interviewId}.pdf";
        var result = await SendAsync(type, fileName, data);
        return (result, null);
    }

    public async Task<(DocumentRequestDto? Result, string? Error)> RequestCandidateCardAsync(int candidateId)
    {
        var candidate = await _candidates.GetByIdAsync(candidateId);
        if (candidate == null)
        {
            return (null, "Кандидат не найден");
        }

        var data = new DocumentData
        {
            CandidateName = candidate.FullName,
            Phone = candidate.Phone,
            City = candidate.City,
            Skills = candidate.Skills
                .Select(s => new SkillData { Name = s.Name, Rating = s.Rating })
                .ToList(),
            Interviews = candidate.Interviews
                .Select(i => $"{i.VacancyTitle}, {i.ScheduledAt.ToLocalTime():dd.MM.yyyy HH:mm}, {i.Status}")
                .ToList()
        };

        var fileName = $"{DocumentType.CandidateCard}_{candidateId}.pdf";
        var result = await SendAsync(DocumentType.CandidateCard, fileName, data);
        return (result, null);
    }

    public async Task<DocumentInfoDto?> GetInfoAsync(string id)
    {
        var record = await _store.GetAsync(id);

        if (record == null)
        {
            return null;
        }

        return new DocumentInfoDto
        {
            Id = record.Id,
            Type = record.Type,
            FileName = record.FileName,
            Status = record.Status,
            Error = record.Error
        };
    }

    public async Task<DocumentRecord?> GetFileAsync(string id)
    {
        return await _store.GetAsync(id);
    }

    // создаёт запись в MongoDB и отправляет задачу в Kafka
    private async Task<DocumentRequestDto> SendAsync(DocumentType type, string fileName, DocumentData data)
    {
        var requestId = Guid.NewGuid().ToString();

        await _store.CreatePendingAsync(new DocumentRecord
        {
            Id = requestId,
            Type = type,
            FileName = fileName,
            Status = DocumentStatus.Pending,
            CreatedAt = DateTime.UtcNow
        });

        await _publisher.PublishAsync(_pdfTopic, new PdfRequestMessage
        {
            RequestId = requestId,
            Type = type,
            Data = data
        });

        await _audit.LogAsync("DocumentRequested", $"{type}, файл {fileName}");
        _logger.LogInformation("Запрошен документ {RequestId} типа {Type}", requestId, type);

        return new DocumentRequestDto { RequestId = requestId, Status = DocumentStatus.Pending };
    }
}

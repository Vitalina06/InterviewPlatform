using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class ArchiveService : IArchiveService
{
    private readonly ICandidateRepository _candidates;
    private readonly IInterviewRepository _interviews;
    private readonly ICandidateService _candidateService;
    private readonly IInterviewService _interviewService;
    private readonly IAuditService _audit;
    private readonly ILogger<ArchiveService> _logger;

    public ArchiveService(
        ICandidateRepository candidates,
        IInterviewRepository interviews,
        ICandidateService candidateService,
        IInterviewService interviewService,
        IAuditService audit,
        ILogger<ArchiveService> logger)
    {
        _candidates = candidates;
        _interviews = interviews;
        _candidateService = candidateService;
        _interviewService = interviewService;
        _audit = audit;
        _logger = logger;
    }

    public async Task<bool> SetCandidateArchivedAsync(int id, bool archived)
    {
        var found = await _candidates.SetArchivedAsync(id, archived);

        if (found)
        {
            var action = archived ? "CandidateArchived" : "CandidateRestored";
            _logger.LogInformation("{Action}: кандидат {CandidateId}", action, id);
            await _audit.LogAsync(action, $"Кандидат {id}");
        }

        return found;
    }

    public async Task<bool> SetInterviewArchivedAsync(int id, bool archived)
    {
        var found = await _interviews.SetArchivedAsync(id, archived);

        if (found)
        {
            var action = archived ? "InterviewArchived" : "InterviewRestored";
            _logger.LogInformation("{Action}: собеседование {InterviewId}", action, id);
            await _audit.LogAsync(action, $"Собеседование {id}");
        }

        return found;
    }

    public async Task<List<CandidateDto>> GetArchivedCandidatesAsync()
    {
        return await _candidateService.SearchAsync(null, true);
    }

    public async Task<List<InterviewDto>> GetArchivedInterviewsAsync()
    {
        return await _interviewService.GetListAsync(null, true);
    }
}

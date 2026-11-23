using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;

namespace InterviewPlatform.Api.Services;

public class CandidateService : ICandidateService
{
    private readonly ICandidateRepository _candidates;
    private readonly ILogger<CandidateService> _logger;

    public CandidateService(ICandidateRepository candidates, ILogger<CandidateService> logger)
    {
        _candidates = candidates;
        _logger = logger;
    }

    public async Task<CandidateDto> CreateAsync(CreateCandidateRequest request)
    {
        var candidate = new Candidate
        {
            FullName = request.FullName.Trim(),
            Phone = request.Phone.Trim(),
            City = request.City.Trim(),
            Skills = request.Skills
                .Select(s => new Skill { Name = s.Name.Trim(), Rating = s.Rating })
                .ToList()
        };

        await _candidates.AddAsync(candidate);
        _logger.LogInformation("Создан кандидат {CandidateId}", candidate.Id);

        return ToDto(candidate);
    }

    public async Task<CandidateDto?> GetByIdAsync(int id)
    {
        var candidate = await _candidates.GetByIdAsync(id);
        return candidate == null ? null : ToDto(candidate);
    }

    public async Task<List<CandidateDto>> SearchAsync(string? search)
    {
        var candidates = await _candidates.SearchAsync(search);
        return candidates.Select(ToDto).ToList();
    }

    private static CandidateDto ToDto(Candidate candidate)
    {
        return new CandidateDto
        {
            Id = candidate.Id,
            FullName = candidate.FullName,
            Phone = candidate.Phone,
            City = candidate.City,
            Skills = candidate.Skills
                .Select(s => new SkillDto { Name = s.Name, Rating = s.Rating })
                .ToList()
        };
    }
}

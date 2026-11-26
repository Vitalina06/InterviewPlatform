using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;
using InterviewPlatform.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InterviewPlatform.Tests;

public class InterviewServiceTests
{
    private readonly Mock<IInterviewRepository> _interviews = new();
    private readonly Mock<ICandidateRepository> _candidates = new();
    private readonly Mock<IVacancyRepository> _vacancies = new();

    private InterviewService CreateService()
    {
        return new InterviewService(
            _interviews.Object,
            _candidates.Object,
            _vacancies.Object,
            NullLogger<InterviewService>.Instance);
    }

    private static Vacancy CreateVacancy()
    {
        return new Vacancy
        {
            Id = 1,
            Title = "C# разработчик",
            Competencies = new List<Competency> { new Competency { Id = 1, Name = "C#" } }
        };
    }

    private static CreateInterviewRequest CreateRequest(int competencyId)
    {
        return new CreateInterviewRequest
        {
            CandidateId = 1,
            VacancyId = 1,
            ScheduledAt = DateTime.UtcNow.AddDays(1),
            Stages = new List<string> { "Скрининг" },
            Ratings = new List<RatingInput> { new RatingInput { CompetencyId = competencyId, Score = 4 } }
        };
    }

    [Fact]
    public async Task SetDecision_Pending_ReturnsError()
    {
        var service = CreateService();

        var result = await service.SetDecisionAsync(1, InterviewStatus.Pending);

        Assert.Null(result.Interview);
        Assert.NotNull(result.Error);
        _interviews.Verify(r => r.SaveAsync(), Times.Never);
    }

    [Fact]
    public async Task SetDecision_InterviewNotFound_ReturnsError()
    {
        _interviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync((Interview?)null);
        var service = CreateService();

        var result = await service.SetDecisionAsync(1, InterviewStatus.Accepted);

        Assert.Null(result.Interview);
        Assert.Contains("не найдено", result.Error);
    }

    [Fact]
    public async Task SetDecision_Valid_ChangesStatusAndSaves()
    {
        var interview = new Interview { Id = 1 };
        _interviews.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(interview);
        var service = CreateService();

        var result = await service.SetDecisionAsync(1, InterviewStatus.Accepted);

        Assert.Null(result.Error);
        Assert.Equal(InterviewStatus.Accepted, interview.Status);
        _interviews.Verify(r => r.SaveAsync(), Times.Once);
    }

    [Fact]
    public async Task Create_ArchivedCandidate_ReturnsError()
    {
        _candidates.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Candidate { Id = 1, IsArchived = true });
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest(1));

        Assert.Contains("в архиве", result.Error);
        _interviews.Verify(r => r.AddAsync(It.IsAny<Interview>()), Times.Never);
    }

    [Fact]
    public async Task Create_CompetencyFromAnotherVacancy_ReturnsError()
    {
        _candidates.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(new Candidate { Id = 1 });
        _vacancies.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(CreateVacancy());
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest(99));

        Assert.Contains("не относится", result.Error);
        _interviews.Verify(r => r.AddAsync(It.IsAny<Interview>()), Times.Never);
    }

    [Fact]
    public async Task Create_Valid_SavesPendingInterviewWithRatings()
    {
        var candidate = new Candidate { Id = 1, FullName = "Петров Иван" };
        var vacancy = CreateVacancy();

        _candidates.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(candidate);
        _vacancies.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(vacancy);
        _interviews.Setup(r => r.GetByIdAsync(It.IsAny<int>())).ReturnsAsync(new Interview
        {
            Id = 5,
            CandidateId = 1,
            Candidate = candidate,
            VacancyId = 1,
            Vacancy = vacancy,
            ScheduledAt = DateTime.UtcNow
        });
        var service = CreateService();

        var result = await service.CreateAsync(CreateRequest(1));

        Assert.Null(result.Error);
        Assert.Equal("Петров Иван", result.Interview!.CandidateName);
        _interviews.Verify(r => r.AddAsync(It.Is<Interview>(i =>
            i.Status == InterviewStatus.Pending && i.Ratings.Count == 1)), Times.Once);
    }
}

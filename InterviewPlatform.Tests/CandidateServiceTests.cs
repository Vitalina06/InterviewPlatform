using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Repositories;
using InterviewPlatform.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace InterviewPlatform.Tests;

public class CandidateServiceTests
{
    [Fact]
    public async Task Create_TrimsSpacesAndSavesSkills()
    {
        var repo = new Mock<ICandidateRepository>();
        var service = new CandidateService(repo.Object, NullLogger<CandidateService>.Instance);

        var request = new CreateCandidateRequest
        {
            FullName = "  Петров Иван  ",
            Phone = "+7 999 123-45-67",
            City = " Москва ",
            Skills = new List<SkillDto> { new SkillDto { Name = " C# ", Rating = 4 } }
        };

        var dto = await service.CreateAsync(request);

        Assert.Equal("Петров Иван", dto.FullName);
        Assert.Equal("Москва", dto.City);
        Assert.Equal("C#", dto.Skills[0].Name);
        repo.Verify(r => r.AddAsync(It.IsAny<Candidate>()), Times.Once);
    }
}

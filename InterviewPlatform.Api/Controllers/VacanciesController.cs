using InterviewPlatform.Api.DTOs;
using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InterviewPlatform.Api.Controllers;

[ApiController]
[Route("api/vacancies")]
[Authorize(Roles = AppRoles.Hr)]
public class VacanciesController : ControllerBase
{
    private readonly IVacancyService _vacancies;

    public VacanciesController(IVacancyService vacancies)
    {
        _vacancies = vacancies;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _vacancies.GetAllAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateVacancyRequest request)
    {
        return Ok(await _vacancies.CreateAsync(request));
    }
}

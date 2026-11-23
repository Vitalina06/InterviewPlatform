using InterviewPlatform.Api.Data;
using InterviewPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Repositories;

public class VacancyRepository : IVacancyRepository
{
    private readonly AppDbContext _db;

    public VacancyRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Competency>> GetCompetenciesByNamesAsync(List<string> names)
    {
        return await _db.Competencies.Where(c => names.Contains(c.Name)).ToListAsync();
    }

    public async Task AddAsync(Vacancy vacancy)
    {
        _db.Vacancies.Add(vacancy);
        await _db.SaveChangesAsync();
    }

    public async Task<List<Vacancy>> GetAllAsync()
    {
        return await _db.Vacancies
            .Include(v => v.Competencies)
            .AsNoTracking()
            .OrderBy(v => v.Title)
            .ToListAsync();
    }

    // с отслеживанием: компетенции потом используются при создании оценок
    public async Task<Vacancy?> GetByIdAsync(int id)
    {
        return await _db.Vacancies
            .Include(v => v.Competencies)
            .FirstOrDefaultAsync(v => v.Id == id);
    }
}

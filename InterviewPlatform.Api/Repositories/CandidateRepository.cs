using InterviewPlatform.Api.Data;
using InterviewPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Repositories;

public class CandidateRepository : ICandidateRepository
{
    private readonly AppDbContext _db;

    public CandidateRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Candidate candidate)
    {
        _db.Candidates.Add(candidate);
        await _db.SaveChangesAsync();
    }

    public async Task<Candidate?> GetByIdAsync(int id)
    {
        return await _db.Candidates
            .Include(c => c.Skills)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Candidate>> SearchAsync(string? search)
    {
        var query = _db.Candidates.Include(c => c.Skills).AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ILike - поиск без учёта регистра, часть ФИО подойдёт
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.FullName, pattern));
        }

        return await query.OrderBy(c => c.FullName).ToListAsync();
    }
}

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
        return await WithDetails().FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task<List<Candidate>> SearchAsync(string? search, bool archived)
    {
        var query = WithDetails().Where(c => c.IsArchived == archived);

        if (!string.IsNullOrWhiteSpace(search))
        {
            // ILike - поиск без учёта регистра, часть ФИО подойдёт
            var pattern = $"%{search.Trim()}%";
            query = query.Where(c => EF.Functions.ILike(c.FullName, pattern));
        }

        return await query.OrderBy(c => c.FullName).ToListAsync();
    }

    // одним запросом меняет флаг, возвращает false, если кандидата нет
    public async Task<bool> SetArchivedAsync(int id, bool archived)
    {
        var rows = await _db.Candidates
            .Where(c => c.Id == id)
            .ExecuteUpdateAsync(s => s.SetProperty(c => c.IsArchived, archived));

        return rows > 0;
    }

    // в карточке кандидата показываем только неархивные собеседования
    private IQueryable<Candidate> WithDetails()
    {
        return _db.Candidates
            .Include(c => c.Skills)
            .Include(c => c.Interviews.Where(i => !i.IsArchived))
            .ThenInclude(i => i.Vacancy)
            .AsSplitQuery()
            .AsNoTracking();
    }
}

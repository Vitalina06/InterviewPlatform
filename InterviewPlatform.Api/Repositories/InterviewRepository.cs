using InterviewPlatform.Api.Data;
using InterviewPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Repositories;

public class InterviewRepository : IInterviewRepository
{
    private readonly AppDbContext _db;

    public InterviewRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task AddAsync(Interview interview)
    {
        _db.Interviews.Add(interview);
        await _db.SaveChangesAsync();
    }

    // с отслеживанием, чтобы можно было менять оценки и статус
    public async Task<Interview?> GetByIdAsync(int id)
    {
        return await WithDetails().FirstOrDefaultAsync(i => i.Id == id);
    }

    public async Task<List<Interview>> GetListAsync(int? candidateId)
    {
        var query = WithDetails().AsNoTracking();

        if (candidateId != null)
        {
            query = query.Where(i => i.CandidateId == candidateId);
        }

        return await query.OrderByDescending(i => i.ScheduledAt).ToListAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }

    private IQueryable<Interview> WithDetails()
    {
        return _db.Interviews
            .Include(i => i.Candidate)
            .Include(i => i.Vacancy)
            .Include(i => i.Stages)
            .Include(i => i.Ratings)
            .ThenInclude(r => r.Competency)
            .AsSplitQuery();
    }
}

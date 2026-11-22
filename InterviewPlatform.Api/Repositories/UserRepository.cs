using InterviewPlatform.Api.Data;
using InterviewPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<User?> GetByLoginAsync(string login)
    {
        return await _db.Users.FirstOrDefaultAsync(u => u.Login == login);
    }

    public async Task<List<User>> GetAllAsync()
    {
        return await _db.Users.AsNoTracking().OrderBy(u => u.Id).ToListAsync();
    }

    public async Task AddAsync(User user)
    {
        _db.Users.Add(user);
        await _db.SaveChangesAsync();
    }

    public async Task SaveAsync()
    {
        await _db.SaveChangesAsync();
    }
}

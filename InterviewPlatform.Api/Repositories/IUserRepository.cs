using InterviewPlatform.Api.Models;

namespace InterviewPlatform.Api.Repositories;

public interface IUserRepository
{
    Task<User?> GetByLoginAsync(string login);
    Task<List<User>> GetAllAsync();
    Task AddAsync(User user);
    Task SaveAsync();
}

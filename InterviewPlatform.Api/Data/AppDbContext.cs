using InterviewPlatform.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Candidate> Candidates => Set<Candidate>();
    public DbSet<Skill> Skills => Set<Skill>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // логин должен быть уникальным
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

        // при удалении кандидата удаляются и его навыки
        modelBuilder.Entity<Candidate>()
            .HasMany(c => c.Skills)
            .WithOne(s => s.Candidate)
            .HasForeignKey(s => s.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

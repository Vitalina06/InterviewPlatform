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
    public DbSet<Competency> Competencies => Set<Competency>();
    public DbSet<Vacancy> Vacancies => Set<Vacancy>();
    public DbSet<Interview> Interviews => Set<Interview>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().HasIndex(u => u.Login).IsUnique();

        modelBuilder.Entity<Candidate>()
            .HasMany(c => c.Skills)
            .WithOne(s => s.Candidate)
            .HasForeignKey(s => s.CandidateId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Competency>().HasIndex(c => c.Name).IsUnique();

        // вакансия <-> компетенции: связь "многие ко многим"
        modelBuilder.Entity<Vacancy>()
            .HasMany(v => v.Competencies)
            .WithMany(c => c.Vacancies);

        // статус хранится в БД текстом (Accepted), а не числом
        modelBuilder.Entity<Interview>()
            .Property(i => i.Status)
            .HasConversion<string>();

        // вакансию с собеседованиями удалить нельзя
        modelBuilder.Entity<Interview>()
            .HasOne(i => i.Vacancy)
            .WithMany()
            .HasForeignKey(i => i.VacancyId)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CompetencyRating>()
            .HasOne(r => r.Competency)
            .WithMany()
            .HasForeignKey(r => r.CompetencyId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

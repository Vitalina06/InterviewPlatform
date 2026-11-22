using InterviewPlatform.Api.Models;
using InterviewPlatform.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace InterviewPlatform.Api.Data;

public static class DbInitializer
{
    // применяет миграции и создаёт первого администратора, если пользователей ещё нет
    public static void Init(IServiceProvider services)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var hasher = services.GetRequiredService<IPasswordHasher>();
        var config = services.GetRequiredService<IConfiguration>();

        db.Database.Migrate();

        if (db.Users.Any())
        {
            return;
        }

        var admin = new User
        {
            FullName = config["Admin:FullName"]!,
            Login = config["Admin:Login"]!,
            PasswordHash = hasher.Hash(config["Admin:Password"]!),
            Roles = new List<string> { AppRoles.Admin }
        };

        db.Users.Add(admin);
        db.SaveChanges();
    }
}

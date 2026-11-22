namespace InterviewPlatform.Api.Models;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Login { get; set; } = "";
    public string PasswordHash { get; set; } = "";
    public List<string> Roles { get; set; } = new();

    // для блокировки после неудачных попыток входа
    public int FailedLoginCount { get; set; }
    public DateTime? LockoutEnd { get; set; }
}

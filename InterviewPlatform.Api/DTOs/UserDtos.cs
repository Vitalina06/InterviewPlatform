using System.ComponentModel.DataAnnotations;

namespace InterviewPlatform.Api.DTOs;

public class CreateUserRequest
{
    [Required]
    public string FullName { get; set; } = "";

    [Required, MinLength(3)]
    public string Login { get; set; } = "";

    [Required, MinLength(6)]
    public string Password { get; set; } = "";

    [Required, MinLength(1)]
    public List<string> Roles { get; set; } = new();
}

// пользователь без пароля и хеша - наружу их отдавать нельзя
public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = "";
    public string Login { get; set; } = "";
    public List<string> Roles { get; set; } = new();
}

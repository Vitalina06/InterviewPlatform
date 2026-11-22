using System.ComponentModel.DataAnnotations;

namespace InterviewPlatform.Api.DTOs;

public class LoginRequest
{
    [Required]
    public string Login { get; set; } = "";

    [Required]
    public string Password { get; set; } = "";
}

public class LoginResult
{
    public bool Success { get; set; }
    public bool IsLocked { get; set; }
    public string? Token { get; set; }
    public string? Error { get; set; }
}

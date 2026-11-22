namespace InterviewPlatform.Api.Models;

public static class AppRoles
{
    public const string Admin = "Admin";
    public const string Hr = "HR";
    public const string Approver = "Approver";

    public static readonly string[] All = { Admin, Hr, Approver };
}

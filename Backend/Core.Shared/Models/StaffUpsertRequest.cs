namespace Core.Shared.Models;

public class StaffUpsertRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
}

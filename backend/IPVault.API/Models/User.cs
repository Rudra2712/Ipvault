namespace IPVault.API.Models;

public class User
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Staff"; // Admin, Attorney, Staff
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

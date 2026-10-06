namespace IPVault.API.Models;

public class Client
{
    public int ClientId { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Company { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation property: One Client has Many Patents
    public ICollection<Patent> Patents { get; set; } = new List<Patent>();
}

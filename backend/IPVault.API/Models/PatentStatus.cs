namespace IPVault.API.Models;

public class PatentStatus
{
    public int StatusId { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    // Navigation property: One Status has Many Patents
    public ICollection<Patent> Patents { get; set; } = new List<Patent>();
}

namespace IPVault.API.Models;

public class Inventor
{
    public int InventorId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Organization { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string FullName => $"{FirstName} {LastName}".Trim();

    // Navigation property: Many-to-Many via PatentInventor junction
    public ICollection<PatentInventor> PatentInventors { get; set; } = new List<PatentInventor>();
}

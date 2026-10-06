namespace IPVault.API.Models;

public class Patent
{
    public int PatentId { get; set; }
    public string PatentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Abstract { get; set; } = string.Empty;
    public DateTime FilingDate { get; set; }
    public DateTime? PublicationDate { get; set; }
    public DateTime? GrantDate { get; set; }
    public DateTime? ExpiryDate { get; set; }

    // Foreign Keys
    public int StatusId { get; set; }
    public PatentStatus? Status { get; set; }

    public int ClientId { get; set; }
    public Client? Client { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation Properties
    public ICollection<PatentInventor> PatentInventors { get; set; } = new List<PatentInventor>();
    public ICollection<PatentDocument> Documents { get; set; } = new List<PatentDocument>();
}

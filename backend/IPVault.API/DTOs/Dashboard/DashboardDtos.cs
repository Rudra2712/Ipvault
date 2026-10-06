namespace IPVault.API.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int TotalPatents { get; set; }
    public int GrantedPatents { get; set; }
    public int PendingPatents { get; set; }
    public int ExpiredPatents { get; set; }
    public int TotalClients { get; set; }
    public int TotalInventors { get; set; }

    public List<StatusCountDto> PatentCountByStatus { get; set; } = new();
    public List<ClientPatentCountDto> PatentCountByClient { get; set; } = new();
    public List<YearlyFilingCountDto> PatentsFiledPerYear { get; set; } = new();
    public List<UpcomingExpiryDto> UpcomingExpiries { get; set; } = new();
}

public class StatusCountDto
{
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ClientPatentCountDto
{
    public string ClientName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class YearlyFilingCountDto
{
    public int Year { get; set; }
    public int Count { get; set; }
}

public class UpcomingExpiryDto
{
    public int PatentId { get; set; }
    public string PatentNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime? ExpiryDate { get; set; }
    public string ClientName { get; set; } = string.Empty;
    public int DaysRemaining { get; set; }
}

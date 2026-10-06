using Microsoft.EntityFrameworkCore;
using IPVault.API.Data;
using IPVault.API.DTOs.Dashboard;

namespace IPVault.API.Services;

public interface IDashboardService
{
    Task<DashboardSummaryDto> GetSummaryAsync();
}

public class DashboardService : IDashboardService
{
    private readonly ApplicationDbContext _context;

    public DashboardService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardSummaryDto> GetSummaryAsync()
    {
        var totalPatents = await _context.Patents.CountAsync();
        var totalClients = await _context.Clients.CountAsync();
        var totalInventors = await _context.Inventors.CountAsync();

        var grantedPatents = await _context.Patents
            .Include(p => p.Status)
            .CountAsync(p => p.Status != null && p.Status.StatusName == "Granted");

        var pendingStatuses = new[] { "Filed", "Under Examination", "Published", "Draft" };
        var pendingPatents = await _context.Patents
            .Include(p => p.Status)
            .CountAsync(p => p.Status != null && pendingStatuses.Contains(p.Status.StatusName));

        var expiredPatents = await _context.Patents
            .Include(p => p.Status)
            .CountAsync(p => p.Status != null && p.Status.StatusName == "Expired");

        // Patent count by status
        var statusCounts = await _context.PatentStatuses
            .Select(s => new StatusCountDto
            {
                StatusName = s.StatusName,
                Count = s.Patents.Count
            })
            .Where(s => s.Count > 0)
            .OrderByDescending(s => s.Count)
            .ToListAsync();

        // Patent count by client
        var clientCounts = await _context.Clients
            .Select(c => new ClientPatentCountDto
            {
                ClientName = c.ClientName,
                Count = c.Patents.Count
            })
            .OrderByDescending(c => c.Count)
            .Take(8)
            .ToListAsync();

        // Patents filed per year
        var yearlyCounts = await _context.Patents
            .GroupBy(p => p.FilingDate.Year)
            .Select(g => new YearlyFilingCountDto
            {
                Year = g.Key,
                Count = g.Count()
            })
            .OrderBy(y => y.Year)
            .ToListAsync();

        // Upcoming expiries
        var now = DateTime.UtcNow;
        var upcomingExpiries = await _context.Patents
            .Include(p => p.Client)
            .Where(p => p.ExpiryDate.HasValue && p.ExpiryDate >= now)
            .OrderBy(p => p.ExpiryDate)
            .Take(5)
            .Select(p => new UpcomingExpiryDto
            {
                PatentId = p.PatentId,
                PatentNumber = p.PatentNumber,
                Title = p.Title,
                ExpiryDate = p.ExpiryDate,
                ClientName = p.Client != null ? p.Client.ClientName : string.Empty,
                DaysRemaining = (int)((p.ExpiryDate!.Value - now).TotalDays)
            })
            .ToListAsync();

        return new DashboardSummaryDto
        {
            TotalPatents = totalPatents,
            GrantedPatents = grantedPatents,
            PendingPatents = pendingPatents,
            ExpiredPatents = expiredPatents,
            TotalClients = totalClients,
            TotalInventors = totalInventors,
            PatentCountByStatus = statusCounts,
            PatentCountByClient = clientCounts,
            PatentsFiledPerYear = yearlyCounts,
            UpcomingExpiries = upcomingExpiries
        };
    }
}

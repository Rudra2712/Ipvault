using Microsoft.EntityFrameworkCore;
using IPVault.API.Data;
using IPVault.API.DTOs.Patents;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(ApplicationDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == email.Trim().ToLower());
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email.ToLower() == email.Trim().ToLower());
    }
}

public class ClientRepository : Repository<Client>, IClientRepository
{
    public ClientRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Client>> SearchClientsAsync(string? search)
    {
        var query = _context.Clients
            .Include(c => c.Patents)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(c => c.ClientName.ToLower().Contains(term) ||
                                     c.Email.ToLower().Contains(term) ||
                                     c.Company.ToLower().Contains(term) ||
                                     c.Phone.Contains(term));
        }

        return await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
    }

    public async Task<Client?> GetClientWithPatentsAsync(int id)
    {
        return await _context.Clients
            .Include(c => c.Patents)
                .ThenInclude(p => p.Status)
            .FirstOrDefaultAsync(c => c.ClientId == id);
    }

    public async Task<bool> HasAssociatedPatentsAsync(int clientId)
    {
        return await _context.Patents.AnyAsync(p => p.ClientId == clientId);
    }
}

public class InventorRepository : Repository<Inventor>, IInventorRepository
{
    public InventorRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<Inventor>> SearchInventorsAsync(string? search)
    {
        var query = _context.Inventors
            .Include(i => i.PatentInventors)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();
            query = query.Where(i => i.FirstName.ToLower().Contains(term) ||
                                     i.LastName.ToLower().Contains(term) ||
                                     (i.FirstName + " " + i.LastName).ToLower().Contains(term) ||
                                     i.Email.ToLower().Contains(term) ||
                                     i.Organization.ToLower().Contains(term));
        }

        return await query.OrderBy(i => i.LastName).ThenBy(i => i.FirstName).ToListAsync();
    }

    public async Task<Inventor?> GetInventorWithPatentsAsync(int id)
    {
        return await _context.Inventors
            .Include(i => i.PatentInventors)
                .ThenInclude(pi => pi.Patent)
                    .ThenInclude(p => p!.Status)
            .FirstOrDefaultAsync(i => i.InventorId == id);
    }

    public async Task<List<Inventor>> GetInventorsByIdsAsync(IEnumerable<int> ids)
    {
        return await _context.Inventors
            .Where(i => ids.Contains(i.InventorId))
            .ToListAsync();
    }
}

public class PatentStatusRepository : Repository<PatentStatus>, IPatentStatusRepository
{
    public PatentStatusRepository(ApplicationDbContext context) : base(context) { }

    public async Task<PatentStatus?> GetByNameAsync(string name)
    {
        return await _context.PatentStatuses.FirstOrDefaultAsync(s => s.StatusName.ToLower() == name.Trim().ToLower());
    }

    public async Task<bool> StatusNameExistsAsync(string name, int? excludeId = null)
    {
        var query = _context.PatentStatuses.Where(s => s.StatusName.ToLower() == name.Trim().ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(s => s.StatusId != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<bool> HasAssociatedPatentsAsync(int statusId)
    {
        return await _context.Patents.AnyAsync(p => p.StatusId == statusId);
    }
}

public class PatentRepository : Repository<Patent>, IPatentRepository
{
    public PatentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<(IEnumerable<Patent> items, int totalCount)> GetPagedPatentsAsync(PatentFilterDto filter)
    {
        var query = _context.Patents
            .Include(p => p.Status)
            .Include(p => p.Client)
            .Include(p => p.PatentInventors)
                .ThenInclude(pi => pi.Inventor)
            .Include(p => p.Documents)
            .AsQueryable();

        // 1. Search term (Patent Number, Title, Abstract)
        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var term = filter.Search.Trim().ToLower();
            query = query.Where(p => p.PatentNumber.ToLower().Contains(term) ||
                                     p.Title.ToLower().Contains(term) ||
                                     p.Abstract.ToLower().Contains(term));
        }

        // 2. Status filter
        if (filter.StatusId.HasValue && filter.StatusId.Value > 0)
        {
            query = query.Where(p => p.StatusId == filter.StatusId.Value);
        }
        else if (!string.IsNullOrWhiteSpace(filter.Status))
        {
            var statusTerm = filter.Status.Trim().ToLower();
            query = query.Where(p => p.Status != null && p.Status.StatusName.ToLower() == statusTerm);
        }

        // 3. Client filter
        if (filter.ClientId.HasValue && filter.ClientId.Value > 0)
        {
            query = query.Where(p => p.ClientId == filter.ClientId.Value);
        }

        // 4. Filing Year filter
        if (filter.Year.HasValue && filter.Year.Value > 0)
        {
            query = query.Where(p => p.FilingDate.Year == filter.Year.Value);
        }

        var totalCount = await query.CountAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize < 1 ? 10 : filter.PageSize;

        var items = await query
            .OrderByDescending(p => p.FilingDate)
            .ThenByDescending(p => p.PatentId)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, totalCount);
    }

    public async Task<Patent?> GetPatentDetailsAsync(int id)
    {
        return await _context.Patents
            .Include(p => p.Status)
            .Include(p => p.Client)
            .Include(p => p.PatentInventors)
                .ThenInclude(pi => pi.Inventor)
            .Include(p => p.Documents)
            .FirstOrDefaultAsync(p => p.PatentId == id);
    }

    public async Task<bool> PatentNumberExistsAsync(string patentNumber, int? excludeId = null)
    {
        var query = _context.Patents.Where(p => p.PatentNumber.ToLower() == patentNumber.Trim().ToLower());
        if (excludeId.HasValue)
        {
            query = query.Where(p => p.PatentId != excludeId.Value);
        }
        return await query.AnyAsync();
    }

    public async Task<IEnumerable<PatentInventor>> GetPatentInventorsAsync(int patentId)
    {
        return await _context.PatentInventors
            .Include(pi => pi.Inventor)
            .Where(pi => pi.PatentId == patentId)
            .ToListAsync();
    }

    public async Task AddPatentInventorAsync(PatentInventor patentInventor)
    {
        await _context.PatentInventors.AddAsync(patentInventor);
    }

    public void RemovePatentInventor(PatentInventor patentInventor)
    {
        _context.PatentInventors.Remove(patentInventor);
    }

    public async Task<PatentInventor?> GetPatentInventorAsync(int patentId, int inventorId)
    {
        return await _context.PatentInventors
            .Include(pi => pi.Inventor)
            .FirstOrDefaultAsync(pi => pi.PatentId == patentId && pi.InventorId == inventorId);
    }
}

public class PatentDocumentRepository : Repository<PatentDocument>, IPatentDocumentRepository
{
    public PatentDocumentRepository(ApplicationDbContext context) : base(context) { }

    public async Task<IEnumerable<PatentDocument>> GetDocumentsByPatentIdAsync(int patentId)
    {
        return await _context.PatentDocuments
            .Where(d => d.PatentId == patentId)
            .OrderByDescending(d => d.UploadedAt)
            .ToListAsync();
    }
}

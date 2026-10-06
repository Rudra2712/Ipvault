using System.Linq.Expressions;
using IPVault.API.DTOs.Patents;
using IPVault.API.Models;

namespace IPVault.API.Interfaces;

public interface IRepository<T> where T : class
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(int id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task AddAsync(T entity);
    void Update(T entity);
    void Delete(T entity);
    Task<bool> SaveChangesAsync();
}

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
    Task<bool> EmailExistsAsync(string email);
}

public interface IClientRepository : IRepository<Client>
{
    Task<IEnumerable<Client>> SearchClientsAsync(string? search);
    Task<Client?> GetClientWithPatentsAsync(int id);
    Task<bool> HasAssociatedPatentsAsync(int clientId);
}

public interface IInventorRepository : IRepository<Inventor>
{
    Task<IEnumerable<Inventor>> SearchInventorsAsync(string? search);
    Task<Inventor?> GetInventorWithPatentsAsync(int id);
    Task<List<Inventor>> GetInventorsByIdsAsync(IEnumerable<int> ids);
}

public interface IPatentStatusRepository : IRepository<PatentStatus>
{
    Task<PatentStatus?> GetByNameAsync(string name);
    Task<bool> StatusNameExistsAsync(string name, int? excludeId = null);
    Task<bool> HasAssociatedPatentsAsync(int statusId);
}

public interface IPatentRepository : IRepository<Patent>
{
    Task<(IEnumerable<Patent> items, int totalCount)> GetPagedPatentsAsync(PatentFilterDto filter);
    Task<Patent?> GetPatentDetailsAsync(int id);
    Task<bool> PatentNumberExistsAsync(string patentNumber, int? excludeId = null);
    Task<IEnumerable<PatentInventor>> GetPatentInventorsAsync(int patentId);
    Task AddPatentInventorAsync(PatentInventor patentInventor);
    void RemovePatentInventor(PatentInventor patentInventor);
    Task<PatentInventor?> GetPatentInventorAsync(int patentId, int inventorId);
}

public interface IPatentDocumentRepository : IRepository<PatentDocument>
{
    Task<IEnumerable<PatentDocument>> GetDocumentsByPatentIdAsync(int patentId);
}

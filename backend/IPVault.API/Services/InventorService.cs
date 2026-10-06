using IPVault.API.DTOs.Inventors;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Services;

public interface IInventorService
{
    Task<IEnumerable<InventorDto>> GetInventorsAsync(string? search);
    Task<InventorDto> GetInventorByIdAsync(int id);
    Task<InventorDto> CreateInventorAsync(CreateInventorDto dto);
    Task<InventorDto> UpdateInventorAsync(int id, UpdateInventorDto dto);
    Task DeleteInventorAsync(int id);
}

public class InventorService : IInventorService
{
    private readonly IInventorRepository _inventorRepository;

    public InventorService(IInventorRepository inventorRepository)
    {
        _inventorRepository = inventorRepository;
    }

    public async Task<IEnumerable<InventorDto>> GetInventorsAsync(string? search)
    {
        var inventors = await _inventorRepository.SearchInventorsAsync(search);
        return inventors.Select(MapToInventorDto);
    }

    public async Task<InventorDto> GetInventorByIdAsync(int id)
    {
        var inventor = await _inventorRepository.GetInventorWithPatentsAsync(id);
        if (inventor == null)
        {
            throw new NotFoundException($"Inventor with ID {id} was not found.");
        }

        return MapToInventorDto(inventor);
    }

    public async Task<InventorDto> CreateInventorAsync(CreateInventorDto dto)
    {
        var inventor = new Inventor
        {
            FirstName = dto.FirstName.Trim(),
            LastName = dto.LastName.Trim(),
            Email = dto.Email.Trim().ToLower(),
            Phone = dto.Phone?.Trim() ?? string.Empty,
            Organization = dto.Organization?.Trim() ?? string.Empty,
            CreatedAt = DateTime.UtcNow
        };

        await _inventorRepository.AddAsync(inventor);
        await _inventorRepository.SaveChangesAsync();

        return MapToInventorDto(inventor);
    }

    public async Task<InventorDto> UpdateInventorAsync(int id, UpdateInventorDto dto)
    {
        var inventor = await _inventorRepository.GetByIdAsync(id);
        if (inventor == null)
        {
            throw new NotFoundException($"Inventor with ID {id} was not found.");
        }

        inventor.FirstName = dto.FirstName.Trim();
        inventor.LastName = dto.LastName.Trim();
        inventor.Email = dto.Email.Trim().ToLower();
        inventor.Phone = dto.Phone?.Trim() ?? string.Empty;
        inventor.Organization = dto.Organization?.Trim() ?? string.Empty;

        _inventorRepository.Update(inventor);
        await _inventorRepository.SaveChangesAsync();

        return MapToInventorDto(inventor);
    }

    public async Task DeleteInventorAsync(int id)
    {
        var inventor = await _inventorRepository.GetByIdAsync(id);
        if (inventor == null)
        {
            throw new NotFoundException($"Inventor with ID {id} was not found.");
        }

        _inventorRepository.Delete(inventor);
        await _inventorRepository.SaveChangesAsync();
    }

    private static InventorDto MapToInventorDto(Inventor inventor)
    {
        return new InventorDto
        {
            InventorId = inventor.InventorId,
            FirstName = inventor.FirstName,
            LastName = inventor.LastName,
            Email = inventor.Email,
            Phone = inventor.Phone,
            Organization = inventor.Organization,
            CreatedAt = inventor.CreatedAt,
            PatentsCount = inventor.PatentInventors?.Count ?? 0
        };
    }
}

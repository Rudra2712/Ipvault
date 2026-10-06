using IPVault.API.DTOs.PatentStatuses;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Services;

public interface IPatentStatusService
{
    Task<IEnumerable<PatentStatusDto>> GetStatusesAsync();
    Task<PatentStatusDto> GetStatusByIdAsync(int id);
    Task<PatentStatusDto> CreateStatusAsync(CreatePatentStatusDto dto);
    Task<PatentStatusDto> UpdateStatusAsync(int id, UpdatePatentStatusDto dto);
    Task DeleteStatusAsync(int id);
}

public class PatentStatusService : IPatentStatusService
{
    private readonly IPatentStatusRepository _statusRepository;

    public PatentStatusService(IPatentStatusRepository statusRepository)
    {
        _statusRepository = statusRepository;
    }

    public async Task<IEnumerable<PatentStatusDto>> GetStatusesAsync()
    {
        var statuses = await _statusRepository.GetAllAsync();
        return statuses.Select(s => new PatentStatusDto
        {
            StatusId = s.StatusId,
            StatusName = s.StatusName,
            Description = s.Description,
            PatentsCount = s.Patents?.Count ?? 0
        });
    }

    public async Task<PatentStatusDto> GetStatusByIdAsync(int id)
    {
        var status = await _statusRepository.GetByIdAsync(id);
        if (status == null)
        {
            throw new NotFoundException($"Patent status with ID {id} was not found.");
        }

        return new PatentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description,
            PatentsCount = status.Patents?.Count ?? 0
        };
    }

    public async Task<PatentStatusDto> CreateStatusAsync(CreatePatentStatusDto dto)
    {
        if (await _statusRepository.StatusNameExistsAsync(dto.StatusName))
        {
            throw new ConflictException($"Patent status '{dto.StatusName}' already exists.");
        }

        var status = new PatentStatus
        {
            StatusName = dto.StatusName.Trim(),
            Description = dto.Description.Trim()
        };

        await _statusRepository.AddAsync(status);
        await _statusRepository.SaveChangesAsync();

        return new PatentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description,
            PatentsCount = 0
        };
    }

    public async Task<PatentStatusDto> UpdateStatusAsync(int id, UpdatePatentStatusDto dto)
    {
        var status = await _statusRepository.GetByIdAsync(id);
        if (status == null)
        {
            throw new NotFoundException($"Patent status with ID {id} was not found.");
        }

        if (await _statusRepository.StatusNameExistsAsync(dto.StatusName, id))
        {
            throw new ConflictException($"Patent status '{dto.StatusName}' already exists.");
        }

        status.StatusName = dto.StatusName.Trim();
        status.Description = dto.Description.Trim();

        _statusRepository.Update(status);
        await _statusRepository.SaveChangesAsync();

        return new PatentStatusDto
        {
            StatusId = status.StatusId,
            StatusName = status.StatusName,
            Description = status.Description
        };
    }

    public async Task DeleteStatusAsync(int id)
    {
        var status = await _statusRepository.GetByIdAsync(id);
        if (status == null)
        {
            throw new NotFoundException($"Patent status with ID {id} was not found.");
        }

        var hasPatents = await _statusRepository.HasAssociatedPatentsAsync(id);
        if (hasPatents)
        {
            throw new BadRequestException("Cannot delete patent status because it is associated with existing patents.");
        }

        _statusRepository.Delete(status);
        await _statusRepository.SaveChangesAsync();
    }
}

using IPVault.API.DTOs.Common;
using IPVault.API.DTOs.Documents;
using IPVault.API.DTOs.Patents;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Services;

public interface IPatentService
{
    Task<PagedResult<PatentDto>> GetPatentsAsync(PatentFilterDto filter);
    Task<PatentDetailDto> GetPatentByIdAsync(int id);
    Task<PatentDetailDto> CreatePatentAsync(CreatePatentDto dto);
    Task<PatentDetailDto> UpdatePatentAsync(int id, UpdatePatentDto dto);
    Task DeletePatentAsync(int id, string userRole);
    Task<IEnumerable<PatentInventorDto>> GetPatentInventorsAsync(int patentId);
    Task<PatentInventorDto> AssignInventorAsync(int patentId, AssignInventorDto dto);
    Task RemoveInventorAsync(int patentId, int inventorId);
}

public class PatentService : IPatentService
{
    private readonly IPatentRepository _patentRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IInventorRepository _inventorRepository;
    private readonly IPatentStatusRepository _statusRepository;

    public PatentService(
        IPatentRepository patentRepository,
        IClientRepository clientRepository,
        IInventorRepository inventorRepository,
        IPatentStatusRepository statusRepository)
    {
        _patentRepository = patentRepository;
        _clientRepository = clientRepository;
        _inventorRepository = inventorRepository;
        _statusRepository = statusRepository;
    }

    public async Task<PagedResult<PatentDto>> GetPatentsAsync(PatentFilterDto filter)
    {
        var (items, totalCount) = await _patentRepository.GetPagedPatentsAsync(filter);

        var dtos = items.Select(p =>
        {
            var primary = p.PatentInventors.FirstOrDefault(pi => pi.IsPrimaryInventor)?.Inventor ??
                          p.PatentInventors.FirstOrDefault()?.Inventor;

            return new PatentDto
            {
                PatentId = p.PatentId,
                PatentNumber = p.PatentNumber,
                Title = p.Title,
                FilingDate = p.FilingDate,
                GrantDate = p.GrantDate,
                ExpiryDate = p.ExpiryDate,
                StatusId = p.StatusId,
                StatusName = p.Status?.StatusName ?? string.Empty,
                ClientId = p.ClientId,
                ClientName = p.Client?.ClientName ?? string.Empty,
                PrimaryInventorName = primary != null ? $"{primary.FirstName} {primary.LastName}".Trim() : "Unassigned",
                InventorsCount = p.PatentInventors.Count,
                DocumentsCount = p.Documents.Count
            };
        });

        return new PagedResult<PatentDto>
        {
            Items = dtos,
            Page = filter.Page < 1 ? 1 : filter.Page,
            PageSize = filter.PageSize < 1 ? 10 : filter.PageSize,
            TotalItems = totalCount
        };
    }

    public async Task<PatentDetailDto> GetPatentByIdAsync(int id)
    {
        var patent = await _patentRepository.GetPatentDetailsAsync(id);
        if (patent == null)
        {
            throw new NotFoundException($"Patent with ID {id} was not found.");
        }

        return MapToPatentDetailDto(patent);
    }

    public async Task<PatentDetailDto> CreatePatentAsync(CreatePatentDto dto)
    {
        // Business Rule 1: Expiry date cannot be before filing date
        if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value < dto.FilingDate)
        {
            throw new BadRequestException("Expiry date cannot be earlier than filing date.");
        }

        // Business Rule 4: Patent number must be unique
        if (await _patentRepository.PatentNumberExistsAsync(dto.PatentNumber))
        {
            throw new ConflictException($"A patent with number '{dto.PatentNumber}' already exists.");
        }

        // Validate Client exists
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID {dto.ClientId} does not exist.");
        }

        // Validate Status exists
        var status = await _statusRepository.GetByIdAsync(dto.StatusId);
        if (status == null)
        {
            throw new NotFoundException($"Patent status with ID {dto.StatusId} does not exist.");
        }

        // Business Rule 2: A patent must have at least one inventor
        if (dto.InventorIds == null || !dto.InventorIds.Any())
        {
            throw new BadRequestException("A patent must have at least one inventor assigned.");
        }

        var distinctInventorIds = dto.InventorIds.Distinct().ToList();
        var inventors = await _inventorRepository.GetInventorsByIdsAsync(distinctInventorIds);
        if (inventors.Count != distinctInventorIds.Count)
        {
            throw new BadRequestException("One or more selected inventors do not exist.");
        }

        var patent = new Patent
        {
            PatentNumber = dto.PatentNumber.Trim().ToUpper(),
            Title = dto.Title.Trim(),
            Abstract = dto.Abstract?.Trim() ?? string.Empty,
            FilingDate = dto.FilingDate,
            PublicationDate = dto.PublicationDate,
            GrantDate = dto.GrantDate,
            ExpiryDate = dto.ExpiryDate,
            StatusId = dto.StatusId,
            ClientId = dto.ClientId,
            CreatedAt = DateTime.UtcNow
        };

        // Determine primary inventor ID
        int primaryId = dto.PrimaryInventorId.HasValue && distinctInventorIds.Contains(dto.PrimaryInventorId.Value)
            ? dto.PrimaryInventorId.Value
            : distinctInventorIds.First();

        foreach (var invId in distinctInventorIds)
        {
            patent.PatentInventors.Add(new PatentInventor
            {
                InventorId = invId,
                IsPrimaryInventor = (invId == primaryId)
            });
        }

        await _patentRepository.AddAsync(patent);
        await _patentRepository.SaveChangesAsync();

        return await GetPatentByIdAsync(patent.PatentId);
    }

    public async Task<PatentDetailDto> UpdatePatentAsync(int id, UpdatePatentDto dto)
    {
        var patent = await _patentRepository.GetByIdAsync(id);
        if (patent == null)
        {
            throw new NotFoundException($"Patent with ID {id} was not found.");
        }

        // Business Rule 1: Expiry date cannot be before filing date
        if (dto.ExpiryDate.HasValue && dto.ExpiryDate.Value < dto.FilingDate)
        {
            throw new BadRequestException("Expiry date cannot be earlier than filing date.");
        }

        // Business Rule 4: Unique patent number check
        if (await _patentRepository.PatentNumberExistsAsync(dto.PatentNumber, id))
        {
            throw new ConflictException($"A patent with number '{dto.PatentNumber}' already exists.");
        }

        // Validate Client exists
        var client = await _clientRepository.GetByIdAsync(dto.ClientId);
        if (client == null)
        {
            throw new NotFoundException($"Client with ID {dto.ClientId} does not exist.");
        }

        // Validate Status exists
        var status = await _statusRepository.GetByIdAsync(dto.StatusId);
        if (status == null)
        {
            throw new NotFoundException($"Patent status with ID {dto.StatusId} does not exist.");
        }

        patent.PatentNumber = dto.PatentNumber.Trim().ToUpper();
        patent.Title = dto.Title.Trim();
        patent.Abstract = dto.Abstract?.Trim() ?? string.Empty;
        patent.FilingDate = dto.FilingDate;
        patent.PublicationDate = dto.PublicationDate;
        patent.GrantDate = dto.GrantDate;
        patent.ExpiryDate = dto.ExpiryDate;
        patent.StatusId = dto.StatusId;
        patent.ClientId = dto.ClientId;
        patent.UpdatedAt = DateTime.UtcNow;

        _patentRepository.Update(patent);
        await _patentRepository.SaveChangesAsync();

        return await GetPatentByIdAsync(id);
    }

    public async Task DeletePatentAsync(int id, string userRole)
    {
        // Business Rule 3: Only Admin can delete a patent
        if (!string.Equals(userRole, "Admin", StringComparison.OrdinalIgnoreCase))
        {
            throw new ForbiddenException("Only administrators have permission to delete patents.");
        }

        var patent = await _patentRepository.GetByIdAsync(id);
        if (patent == null)
        {
            throw new NotFoundException($"Patent with ID {id} was not found.");
        }

        _patentRepository.Delete(patent);
        await _patentRepository.SaveChangesAsync();
    }

    public async Task<IEnumerable<PatentInventorDto>> GetPatentInventorsAsync(int patentId)
    {
        var inventors = await _patentRepository.GetPatentInventorsAsync(patentId);
        return inventors.Select(pi => new PatentInventorDto
        {
            InventorId = pi.InventorId,
            FullName = pi.Inventor != null ? $"{pi.Inventor.FirstName} {pi.Inventor.LastName}".Trim() : string.Empty,
            Email = pi.Inventor?.Email ?? string.Empty,
            Organization = pi.Inventor?.Organization ?? string.Empty,
            IsPrimaryInventor = pi.IsPrimaryInventor
        });
    }

    public async Task<PatentInventorDto> AssignInventorAsync(int patentId, AssignInventorDto dto)
    {
        var patent = await _patentRepository.GetByIdAsync(patentId);
        if (patent == null)
        {
            throw new NotFoundException($"Patent with ID {patentId} was not found.");
        }

        var inventor = await _inventorRepository.GetByIdAsync(dto.InventorId);
        if (inventor == null)
        {
            throw new NotFoundException($"Inventor with ID {dto.InventorId} was not found.");
        }

        var existing = await _patentRepository.GetPatentInventorAsync(patentId, dto.InventorId);
        if (existing != null)
        {
            existing.IsPrimaryInventor = dto.IsPrimaryInventor;
            if (dto.IsPrimaryInventor)
            {
                var others = await _patentRepository.GetPatentInventorsAsync(patentId);
                foreach (var other in others.Where(o => o.InventorId != dto.InventorId))
                {
                    other.IsPrimaryInventor = false;
                }
            }
            await _patentRepository.SaveChangesAsync();

            return new PatentInventorDto
            {
                InventorId = inventor.InventorId,
                FullName = $"{inventor.FirstName} {inventor.LastName}".Trim(),
                Email = inventor.Email,
                Organization = inventor.Organization,
                IsPrimaryInventor = existing.IsPrimaryInventor
            };
        }

        if (dto.IsPrimaryInventor)
        {
            var others = await _patentRepository.GetPatentInventorsAsync(patentId);
            foreach (var other in others)
            {
                other.IsPrimaryInventor = false;
            }
        }

        var patentInventor = new PatentInventor
        {
            PatentId = patentId,
            InventorId = dto.InventorId,
            IsPrimaryInventor = dto.IsPrimaryInventor
        };

        await _patentRepository.AddPatentInventorAsync(patentInventor);
        await _patentRepository.SaveChangesAsync();

        return new PatentInventorDto
        {
            InventorId = inventor.InventorId,
            FullName = $"{inventor.FirstName} {inventor.LastName}".Trim(),
            Email = inventor.Email,
            Organization = inventor.Organization,
            IsPrimaryInventor = patentInventor.IsPrimaryInventor
        };
    }

    public async Task RemoveInventorAsync(int patentId, int inventorId)
    {
        var existing = await _patentRepository.GetPatentInventorAsync(patentId, inventorId);
        if (existing == null)
        {
            throw new NotFoundException("Inventor is not assigned to this patent.");
        }

        var allInventors = (await _patentRepository.GetPatentInventorsAsync(patentId)).ToList();
        if (allInventors.Count <= 1)
        {
            throw new BadRequestException("Cannot remove inventor. A patent must have at least one inventor.");
        }

        _patentRepository.RemovePatentInventor(existing);

        // If removing the primary inventor, promote another inventor
        if (existing.IsPrimaryInventor)
        {
            var next = allInventors.FirstOrDefault(i => i.InventorId != inventorId);
            if (next != null)
            {
                next.IsPrimaryInventor = true;
            }
        }

        await _patentRepository.SaveChangesAsync();
    }

    private static PatentDetailDto MapToPatentDetailDto(Patent patent)
    {
        return new PatentDetailDto
        {
            PatentId = patent.PatentId,
            PatentNumber = patent.PatentNumber,
            Title = patent.Title,
            Abstract = patent.Abstract,
            FilingDate = patent.FilingDate,
            PublicationDate = patent.PublicationDate,
            GrantDate = patent.GrantDate,
            ExpiryDate = patent.ExpiryDate,
            StatusId = patent.StatusId,
            StatusName = patent.Status?.StatusName ?? string.Empty,
            StatusDescription = patent.Status?.Description ?? string.Empty,
            ClientId = patent.ClientId,
            ClientName = patent.Client?.ClientName ?? string.Empty,
            ClientCompany = patent.Client?.Company ?? string.Empty,
            ClientEmail = patent.Client?.Email ?? string.Empty,
            ClientPhone = patent.Client?.Phone ?? string.Empty,
            CreatedAt = patent.CreatedAt,
            UpdatedAt = patent.UpdatedAt,
            Inventors = patent.PatentInventors.Select(pi => new PatentInventorDto
            {
                InventorId = pi.InventorId,
                FullName = pi.Inventor != null ? $"{pi.Inventor.FirstName} {pi.Inventor.LastName}".Trim() : string.Empty,
                Email = pi.Inventor?.Email ?? string.Empty,
                Organization = pi.Inventor?.Organization ?? string.Empty,
                IsPrimaryInventor = pi.IsPrimaryInventor
            }).OrderByDescending(i => i.IsPrimaryInventor).ToList(),
            Documents = patent.Documents.Select(d => new PatentDocumentDto
            {
                DocumentId = d.DocumentId,
                PatentId = d.PatentId,
                DocumentName = d.DocumentName,
                DocumentType = d.DocumentType,
                FilePath = d.FilePath,
                FileSize = d.FileSize,
                UploadedAt = d.UploadedAt,
                UploadedBy = d.UploadedBy
            }).OrderByDescending(d => d.UploadedAt).ToList()
        };
    }
}

using Microsoft.AspNetCore.Hosting;
using IPVault.API.DTOs.Documents;
using IPVault.API.Helpers;
using IPVault.API.Interfaces;
using IPVault.API.Models;

namespace IPVault.API.Services;

public interface IDocumentService
{
    Task<IEnumerable<PatentDocumentDto>> GetDocumentsByPatentIdAsync(int patentId);
    Task<PatentDocumentDto> UploadDocumentAsync(int patentId, UploadDocumentDto dto, string uploadedBy);
    Task DeleteDocumentAsync(int documentId);
    Task<(byte[] fileBytes, string contentType, string fileName)> DownloadDocumentAsync(int documentId);
}

public class DocumentService : IDocumentService
{
    private readonly IPatentDocumentRepository _documentRepository;
    private readonly IPatentRepository _patentRepository;
    private readonly IWebHostEnvironment _environment;

    public DocumentService(
        IPatentDocumentRepository documentRepository,
        IPatentRepository patentRepository,
        IWebHostEnvironment environment)
    {
        _documentRepository = documentRepository;
        _patentRepository = patentRepository;
        _environment = environment;
    }

    public async Task<IEnumerable<PatentDocumentDto>> GetDocumentsByPatentIdAsync(int patentId)
    {
        var docs = await _documentRepository.GetDocumentsByPatentIdAsync(patentId);
        return docs.Select(d => new PatentDocumentDto
        {
            DocumentId = d.DocumentId,
            PatentId = d.PatentId,
            DocumentName = d.DocumentName,
            DocumentType = d.DocumentType,
            FilePath = d.FilePath,
            FileSize = d.FileSize,
            UploadedAt = d.UploadedAt,
            UploadedBy = d.UploadedBy
        });
    }

    public async Task<PatentDocumentDto> UploadDocumentAsync(int patentId, UploadDocumentDto dto, string uploadedBy)
    {
        var patent = await _patentRepository.GetByIdAsync(patentId);
        if (patent == null)
        {
            throw new NotFoundException($"Patent with ID {patentId} was not found.");
        }

        if (dto.File == null || dto.File.Length == 0)
        {
            throw new BadRequestException("No file was uploaded.");
        }

        var uploadsFolder = Path.Combine(_environment.ContentRootPath, "uploads");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = $"{patent.PatentNumber}_{Guid.NewGuid():N}_{Path.GetFileName(dto.File.FileName)}";
        var fullPath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await dto.File.CopyToAsync(stream);
        }

        var doc = new PatentDocument
        {
            PatentId = patentId,
            DocumentName = dto.File.FileName,
            DocumentType = dto.DocumentType,
            FilePath = Path.Combine("uploads", uniqueFileName).Replace("\\", "/"),
            FileSize = dto.File.Length,
            UploadedAt = DateTime.UtcNow,
            UploadedBy = uploadedBy
        };

        await _documentRepository.AddAsync(doc);
        await _documentRepository.SaveChangesAsync();

        return new PatentDocumentDto
        {
            DocumentId = doc.DocumentId,
            PatentId = doc.PatentId,
            DocumentName = doc.DocumentName,
            DocumentType = doc.DocumentType,
            FilePath = doc.FilePath,
            FileSize = doc.FileSize,
            UploadedAt = doc.UploadedAt,
            UploadedBy = doc.UploadedBy
        };
    }

    public async Task DeleteDocumentAsync(int documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null)
        {
            throw new NotFoundException($"Document with ID {documentId} was not found.");
        }

        var fullPath = Path.Combine(_environment.ContentRootPath, doc.FilePath);
        if (File.Exists(fullPath))
        {
            try
            {
                File.Delete(fullPath);
            }
            catch
            {
                // Continue deletion of DB record
            }
        }

        _documentRepository.Delete(doc);
        await _documentRepository.SaveChangesAsync();
    }

    public async Task<(byte[] fileBytes, string contentType, string fileName)> DownloadDocumentAsync(int documentId)
    {
        var doc = await _documentRepository.GetByIdAsync(documentId);
        if (doc == null)
        {
            throw new NotFoundException($"Document with ID {documentId} was not found.");
        }

        var fullPath = Path.Combine(_environment.ContentRootPath, doc.FilePath);
        byte[] bytes;
        if (File.Exists(fullPath))
        {
            bytes = await File.ReadAllBytesAsync(fullPath);
        }
        else
        {
            // Sample placeholder byte content for seeded mock documents
            bytes = System.Text.Encoding.UTF8.GetBytes($"IPVault Patent Document: {doc.DocumentName}\nType: {doc.DocumentType}\nPatent ID: {doc.PatentId}\nUploaded by: {doc.UploadedBy}");
        }

        var contentType = GetMimeType(doc.DocumentName);
        return (bytes, contentType, doc.DocumentName);
    }

    private static string GetMimeType(string fileName)
    {
        var ext = Path.GetExtension(fileName).ToLowerInvariant();
        return ext switch
        {
            ".pdf" => "application/pdf",
            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
            ".doc" => "application/msword",
            ".png" => "image/png",
            ".jpg" or ".jpeg" => "image/jpeg",
            ".txt" => "text/plain",
            _ => "application/octet-stream"
        };
    }
}

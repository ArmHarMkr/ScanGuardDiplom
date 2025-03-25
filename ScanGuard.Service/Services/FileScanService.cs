using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using ScanGuard.Domain.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Services;

public class FileScanService : IFileScanService
{
    private readonly RestClient _restClient;
    private readonly string _virusTotalApiKey = "d385569c8812fd914740dd26544e1d01ca87efc433ac7c44e4dfc0ff0689cb97"; // Замените на ваш ключ
    private readonly ApplicationDbContext _context;

    public FileScanService(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
        var options = new RestClientOptions("https://www.virustotal.com/api/v3/")
        {
            ThrowOnAnyError = true
        };
        _restClient = new RestClient(options) ?? throw new InvalidOperationException("Failed to initialize RestClient");
    }

    public async Task<int> GetDailyScanCountAsync(string userId) 
    {
        var today = DateTime.UtcNow.Date;
        return await _context.FileScanEntities
            .CountAsync(x => x.ApplicationUserId == userId && x.ScanDate.Date == today);
    }

    public async Task<List<FileScanEntity>> GetUserScanHistoryAsync(string userId) 
    {
        return await _context.FileScanEntities
            .Where(x => x.ApplicationUserId == userId)
            .OrderByDescending(x => x.ScanDate)
            .ToListAsync();
    }

    // Остальной код без изменений
    public async Task<FileScanEntity> ScanFileAsync(IFormFile file, string userId)
    {
        if (file == null) throw new ArgumentNullException(nameof(file));

        var scanEntity = new FileScanEntity
        {
            FileName = file.FileName,
            ScanDate = DateTime.UtcNow,
            ApplicationUserId = userId
        };

        using (var stream = file.OpenReadStream())
        {
            scanEntity.FileHash = await CalculateFileHashAsync(stream);
            scanEntity.VulnerabilityType = await CheckFileContent(stream, file.FileName);
            scanEntity.IsMalicious = scanEntity.VulnerabilityType != VulnerabilityType.NotFound;
        }

        var vtReport = await ScanWithVirusTotal(file);
        scanEntity.VirusTotalReport = vtReport;

        await _context.FileScanEntities.AddAsync(scanEntity);
        await _context.SaveChangesAsync();

        return scanEntity;
    }

    private async Task<string> CalculateFileHashAsync(Stream stream)
    {
        using (var sha256 = SHA256.Create())
        {
            byte[] hash = await sha256.ComputeHashAsync(stream);
            return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }
    }

    private async Task<VulnerabilityType> CheckFileContent(Stream stream, string fileName)
    {
        string extension = Path.GetExtension(fileName)?.ToLower() ?? string.Empty;
        if (extension == ".exe" || extension == ".bat")
            return VulnerabilityType.Virus;
        if (extension == ".docm" || extension == ".xlsm")
            return VulnerabilityType.Macro;
        return VulnerabilityType.NotFound;
    }

    private async Task<string> ScanWithVirusTotal(IFormFile file)
    {
        if (_restClient == null)
            throw new InvalidOperationException("RestClient is not initialized");

        var request = new RestRequest("files", Method.Post);
        request.AddHeader("accept", "application/json");
        request.AddHeader("x-apikey", _virusTotalApiKey);

        using (var stream = file.OpenReadStream())
        using (var memoryStream = new MemoryStream())
        {
            await stream.CopyToAsync(memoryStream);
            request.AddFile("file", memoryStream.ToArray(), file.FileName);
        }

        var response = await _restClient.ExecuteAsync(request);
        if (!response.IsSuccessful)
        {
            return $"Error: {response.ErrorMessage ?? "Unknown error from VirusTotal"}";
        }

        return response.Content ?? "No content returned from VirusTotal";
    }

    public async Task<FileScanEntity> GetScanResultAsync(string id)
    {
        return await _context.FileScanEntities
            .Include(x => x.ScannedByUser)
            .FirstOrDefaultAsync(x => x.Id == id);
    }
}
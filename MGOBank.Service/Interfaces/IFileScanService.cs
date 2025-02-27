using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Interfaces
{
    public interface IFileScanService
    {
        Task<FileScanEntity> ScanFileAsync(IFormFile file, string userId);
        Task<FileScanEntity> GetScanResultAsync(string id);
        Task<int> GetDailyScanCountAsync(string userId); // Изменено на Async
        Task<List<FileScanEntity>> GetUserScanHistoryAsync(string userId); // Добавлено    }
    }
}
using HtmlAgilityPack;
using MGOBankApp.Domain.Entity;
using MGOBankApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace MGOBankApp.Service.Interfaces
{
    public interface IScannerService
    {
        Task<Vulnerability> ScanUrl(string url, ApplicationUser? applicationUser);
    }
}

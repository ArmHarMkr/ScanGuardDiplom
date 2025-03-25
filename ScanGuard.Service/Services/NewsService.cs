using ScanGuard.BLL.Interfaces;
using ScanGuard.DAL.Data;
using ScanGuard.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ScanGuard.BLL.Services
{
    public class NewsService : INewsService
    {
        private readonly ApplicationDbContext _context;
        public NewsService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreteNews(NewsEntity news)
        {
            _context.NewsEntities.Add(news);
            await _context.SaveChangesAsync();
        }

        public async Task<List<NewsEntity>> GetAllNews()
        {
            var list = await _context.NewsEntities.ToListAsync() ?? new List<NewsEntity>();
            return list;
        }

        public async Task RemoveNews(string id)
        {
            var news = await _context.NewsEntities.FirstOrDefaultAsync(x => x.Id == id);
            if (news != null)
            {
                _context.NewsEntities.Remove(news);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateNews(NewsEntity news)
        {
            if (news != null)
            {
                _context.NewsEntities.Update(news);
                await _context.SaveChangesAsync();
            }
        }
    }
}
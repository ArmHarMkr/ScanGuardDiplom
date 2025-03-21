﻿using MGOBankApp.Domain.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Interfaces
{
    public interface INewsService
    {
        Task CreteNews(NewsEntity news);
        Task UpdateNews(NewsEntity news);
        Task RemoveNews(string id);
        Task<List<NewsEntity>> GetAllNews();
    }
}

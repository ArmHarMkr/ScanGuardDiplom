using MGOBankApp.DAL.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MGOBankApp.BLL.Services
{
    public class TGUserService
    {
        private readonly ApplicationDbContext _context;

        public TGUserService(ApplicationDbContext context)
        {
            _context = context;
        }

    }
}

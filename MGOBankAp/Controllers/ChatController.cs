using Microsoft.AspNetCore.Mvc;

namespace MGOBankApp.Controllers
{
    public class ChatController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

using MGOBankApp.Domain.Entity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MGOBankAp.Controllers
{
    public class HomeController : Controller
    {
        public HomeController()
        {
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult AboutUs()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Scanner()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Scanner(string url)
        {
            if (!string.IsNullOrEmpty(url))
            {
                Console.WriteLine($"Полученная ссылка: {url}");
                ViewBag.ReceivedUrl = url; // Передаём в представление
            }
            return View();
        }

    }
}
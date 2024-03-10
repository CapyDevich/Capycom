using Capycom.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Capycom.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly CapycomContext db;

        public HomeController(ILogger<HomeController> logger, CapycomContext db)
        {
            _logger = logger;
            this.db = db;
        }

        public IActionResult Index()
        {
            Debug.WriteLine(db.Database.CanConnect());
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }
        public IActionResult LogIn()
        {
            return View();
        }
        public IActionResult Profile()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

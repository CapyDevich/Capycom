using Capycom.Models;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Serilog;
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
            return View();
        }

        public IActionResult Manual()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
			var exceptionHandlerPathFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
			Log.Fatal(exceptionHandlerPathFeature.Error, "An unhandled exception occurred. {@0}", exceptionHandlerPathFeature);
			return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

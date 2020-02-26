using System.Diagnostics;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using WebBlobLogging.Models;

namespace WebBlobLogging.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
            _logger.LogInformation("In HomeController");
        }

        public IActionResult Index()
        {
            _logger.LogInformation("In HomeController.Index");
            return View();
        }

        public IActionResult Privacy()
        {
            _logger.LogInformation("In HomeController.Privacy");
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

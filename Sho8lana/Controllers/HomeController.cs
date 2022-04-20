using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using System.Diagnostics;

namespace Sho8lana.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        private readonly IUnitOfWork _context;

        public HomeController(ILogger<HomeController> logger,IUnitOfWork context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var categories = await _context.Categories.GetAll();
            return View(categories);

        }

        public async Task<IActionResult> Privacy()
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
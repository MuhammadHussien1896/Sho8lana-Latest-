using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    public class CategoriesController : Controller
    {
        private readonly IUnitOfWork _context;

        public CategoriesController(IUnitOfWork context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(int id)
        {
            var services =await _context.Services.GetAllByCategory(id);
            return View(services);
        }
        
        public async Task<IActionResult> Search(string text)
        {
            var services = await _context.Services.GetAllBy(s => s.Description.Contains(text));
            return View("index",services);
        }
    }
}

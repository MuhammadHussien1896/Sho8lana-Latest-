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

            var services = await _context.Services.GetAllBy(s => s.IsFreelancer && s.IsAccepted && s.CategoryId == id);
            ViewBag.Categories = await _context.Categories.GetAll();
            ViewBag.Id = id;
            return View(services);
        }

        public async Task<IActionResult> Search(string text)
        {
            ViewBag.Text = text;
            ViewBag.Categories = await _context.Categories.GetAll();
            var services = await _context.Services
                .GetAllBy(s => s.Description.Contains(text)&& s.IsFreelancer && s.IsAccepted);
            if (services == null) return NotFound();
            return View(services);
        }
        [HttpGet]
        [ActionName("Search")]
        public async Task<IActionResult> SearchByCategory(string text, int? categoryId)
        {
            ViewBag.Text = text;
            ViewBag.Id = categoryId;
            ViewBag.Categories = await _context.Categories.GetAll();
            if (categoryId == null)
            {
                var services = await _context.Services
                    .GetAllBy(s => s.Description.Contains(text)&& s.IsFreelancer && s.IsAccepted);
                if (services == null) return NotFound();
                return View(services);
            }
            else
            {
                var services = await _context.Services
                    .GetAllBy(s => s.Description.Contains(text) &&s.IsFreelancer && s.IsAccepted && s.Category.CategoryId == categoryId);
                if (services == null) return NotFound();
                return View(services);
            }
        }
        public async Task<IActionResult> Hiring(int id)
        {
            var services = await _context.Services.GetAllBy(s => s.IsFreelancer==false && s.IsAccepted && s.CategoryId == id);
            ViewBag.Categories = await _context.Categories.GetAll();
            ViewBag.Id = id;
            return View(services);

        }
    }
}


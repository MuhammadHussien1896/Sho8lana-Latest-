using Microsoft.AspNetCore.Mvc;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _context;

        public AdminController(IUnitOfWork context)
        {
            _context = context;
        }


        public IActionResult Index()
        {
            return View();
        }
    }
}

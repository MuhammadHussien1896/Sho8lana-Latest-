using Microsoft.AspNetCore.Mvc;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    public class CustomerController : Controller
    {
        private readonly IUnitOfWork context;

        public CustomerController(IUnitOfWork context)
        {
            this.context = context;
        }
        public IActionResult Index()
        {
            return View();
        }
         

    }
}

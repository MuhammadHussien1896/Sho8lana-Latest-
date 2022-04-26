using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Sho8lana.Models;
using Sho8lana.Models.ViewModels;
using Sho8lana.Paging;
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

        public async Task<IActionResult> Index(int id,int pg=1)
        {

            var services = await _context.Services.GetAllBy(s => s.IsFreelancer && s.IsAccepted && s.CategoryId == id);
            var categories = await _context.Categories.GetAll();

            //////paging section
            const int PageSize = 16;
            int RecsCount=services.Count();
            var pager = new pagination(RecsCount, pg, PageSize);
            int rescPage = (pg - 1) * PageSize;
            var data=services.OrderByDescending(s=>s.Rate).Skip(rescPage).Take(pager.PageSize).ToList();
            ViewBag.pager=pager;
            //////
            ServicesCategoriesViewModel model = new ServicesCategoriesViewModel()
            {
                services = data,
                categories = categories
            };
            ViewBag.Id = id;
            return View(model);
        }
        public async Task<IActionResult> Search(string text, string type, string Truste, int? CatId , int? Rate, int? Price, int pg = 1)
        {
            ViewBag.Text = text;
            TempData["category"] = CatId;
            TempData["Rate"] = Rate;
            TempData["Price"] = Price;
            TempData["Trusted"] = Truste;
            var categories = await _context.Categories.GetAll();
            IEnumerable<Service> services = new List<Service>();
            if (text == null)
            {
                services = await _context.Services
                                 .GetAllBy(s => s.IsAccepted
                                  && s.IsFreelancer == Convert.ToBoolean(type)
                                  && s.Rate <= (Rate??10) && s.Price <= (Price??int.MaxValue));
            }
            else
            {
                                services = await _context.Services
                                 .GetAllBy(s => s.IsAccepted
                                  && s.IsFreelancer == Convert.ToBoolean(type)
                                  && s.Rate <= (Rate ?? 10) && s.Price <= (Price ?? int.MaxValue)
                                  && s.Description.Contains(text));

            }
            if (!(CatId is null)) services = services.Where(s => s.CategoryId == CatId);
            
            if (!(Truste is null)) services=services.Where(s=>s.Customer.IsVerified==Convert.ToBoolean(Truste));
            
            if(Rate==null&&Price==null) services = services.OrderByDescending(s => s.Rate);
            
            else if (Rate == null) services = services.OrderByDescending(s => s.Price);
            
            else services = services.OrderByDescending(s => s.Rate).OrderByDescending(s => s.Price);

            //////paging section
            const int PageSize = 16;
            int RecsCount = services.Count();
            var pager = new pagination(RecsCount, pg, PageSize);
            int rescPage = (pg - 1) * PageSize;
            var data = services.Skip(rescPage).Take(pager.PageSize).ToList();
            ViewBag.pager = pager;
            //////

            ServicesCategoriesViewModel model = new ServicesCategoriesViewModel()
            {
                services =data,
                categories = categories
            };

            if (services == null) return NotFound();

            else if (type == "true") return View("index", model);
            
            else  return View("hiring", model);
        }
        public async Task<IActionResult> Hiring(int id, int pg = 1)
        {
            var services = await _context.Services.GetAllBy(s => s.IsFreelancer==false && s.IsAccepted && s.CategoryId == id);
            var categories = await _context.Categories.GetAll();
            ////paging section
            const int PageSize = 16;
            int RecsCount = services.Count();
            var pager = new pagination(RecsCount, pg, PageSize);
            int rescPage = (pg - 1) * PageSize;
            var data = services.Skip(rescPage).Take(pager.PageSize).ToList();
            ViewBag.pager = pager;
            ////////
            ServicesCategoriesViewModel model = new ServicesCategoriesViewModel()
            {
                services = data,
                categories = categories
            };
  
            ViewBag.Id = id;
            return View(model);

        }
    }
}


using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
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
        [HttpPost]
        public async Task<IActionResult> VerifyUser(string id)
        {
            if(id == null) { return NotFound(); }
            var user = await _context.Customers.GetById(id);
            if(user == null) { return NotFound(); }
            else
            {
                user.IsVerified = true;
                Notification notification = new Notification()
                {
                    CustomerId = user.Id,
                    Content = "تهانينا , لقد تم التحقق من هويتك الشخصية ",
                    Date = DateTime.Now,
                };
                _context.Notifications.Add(notification);
                await _context.complete();

                return RedirectToAction("Index");
            }
        }
        [HttpPost]
        public async Task<IActionResult> RejectUser(string id)
        {
            if (id == null) { return NotFound(); }
            var user = await _context.Customers.GetById(id);
            if (user == null) { return NotFound(); }
            else
            {
                user.IsVerified = false;
                Notification notification = new Notification()
                {
                    CustomerId = user.Id,
                    Content = "معذرة , لقد تم تعذر التحقق من هويتك الشخصية , حاول مرة اخري",
                    Date = DateTime.Now,
                };
                _context.Notifications.Add(notification);
                await _context.complete();

                return RedirectToAction("Index");
            }
        }

        [HttpGet]
        public async Task<IActionResult> VerifingPage(string id)
        {
            if(id==null) { return NotFound(); }
            var user = await _context.Customers.GetById(id);
            if(user == null) { return NotFound(); }
            else { return View(user); }
        }
    }
}

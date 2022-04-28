using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    [Authorize(Roles ="Admin")]
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

        public async Task<IActionResult> ReviewServices()
        {
            var services = _context.Services.GetAllBy(a => a.IsAccepted == false&&a.IsRejected==false).Result;

            return View(services);
        }

       
        [HttpPost]
        public async Task<IActionResult> UpdateListToActive(List<Service> services)
        {
            try
            {
                _context.Services.UpdateList(services);
                await _context.complete();
                return RedirectToAction(nameof(ReviewServices));

            }
            catch
            {
                return NotFound();
            }
        }

        [HttpPost]
        public async Task<IActionResult> SendAcceptToUser(string UserId,int ServiceId)
        {
            if (UserId == null) { return NotFound(); }
            var user = await _context.Customers.GetById(UserId);
            if (user == null) { return NotFound(); }
            else
            {
                try
                {
                    var service = await _context.Services.GetBy(a => a.ServiceId == ServiceId);
                    service.IsAccepted = true;
                    _context.Services.Update(service);
                    await _context.complete();
                    //// add Notifi
                    ///


                    user.IsVerified = true;
                    Notification notification = new Notification()
                    {
                        CustomerId = user.Id,
                        Content = "تهانينا تمت  مراجعت الخدمة واضافتها بنجاح  ",
                        Date = DateTime.Now,
                    };
                    _context.Notifications.Add(notification);
                    await _context.complete();
                    return RedirectToAction(nameof(ReviewServices));
                }
                catch
                {
                    var service = await _context.Services.GetBy(a => a.ServiceId == ServiceId);
                    service.IsAccepted = false;
                    return View();
                }
            }

        }


        [HttpPost]
        public async Task<IActionResult> SendRejectToUser(string UserId, string messagenotfi,int ServiceId)
        {
            if (UserId == null) { return NotFound(); }
            var user = await _context.Customers.GetById(UserId);
            if (user == null) { return NotFound(); }
            else
            {
                try
                {
                    var service = await _context.Services.GetBy(a => a.ServiceId == ServiceId);
                    service.IsRejected = true;
                    _context.Services.Update(service);
                    await _context.complete();
                    //// add Notifi
                    if (messagenotfi == null) { messagenotfi = "هذة الخدمة لا تطابق السياسة الخاصة بموقع شغلانة نرجو تعديل تلك الخدمة مرة اخرى "; }
                    user.IsVerified = true;
                    Notification notification = new Notification()
                    {
                        CustomerId = user.Id,
                        Content = messagenotfi,
                        Date = DateTime.Now,
                    };
                    _context.Notifications.Add(notification);
                    await _context.complete();

                    return RedirectToAction(nameof(ReviewServices));
                }
                catch
                {
                    var service = await _context.Services.GetBy(a => a.ServiceId == ServiceId);
                    service.IsRejected = false;
                    return View();

                }
            }
        }


        [HttpPost]
        public async Task<IActionResult> VerifyUser(string id)
        {
            if (id == null) { return NotFound(); }
            var user = await _context.Customers.GetById(id);
            if (user == null) { return NotFound(); }
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
            if (id == null) { return NotFound(); }
            var user = await _context.Customers.GetById(id);
            if (user == null) { return NotFound(); }
            else { return View(user); }
        }

    }
}

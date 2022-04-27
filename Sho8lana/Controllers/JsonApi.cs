using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    public class JsonApi : Controller
    {
        private readonly IUnitOfWork context;
        private readonly UserManager<Customer> userManager;

        public JsonApi(IUnitOfWork context, UserManager<Customer> userManager)
        {
            this.context = context;
            this.userManager = userManager;
        }
        [HttpGet]
        public async Task<IActionResult> UnReadNotifcation()
        {
            var customerId = userManager.GetUserId(User);
            var notification = await context.Notifications.GetAllBy(c => c.CustomerId == customerId && c.IsRead == false);
            var notificationCount = notification.ToList().Count;
            if (customerId == null)
            {
                return NotFound();
            };
            return Json(notificationCount);

        }
        [HttpGet]
        public async Task<IActionResult> UnReadNotifcationChange()
        {
            var customerId = userManager.GetUserId(User);
            var notifications = await context.Notifications.GetAllBy(c => c.CustomerId == customerId && c.IsRead == false);
            if (customerId == null)
            {
                return NotFound();
            };
            foreach (var notification in notifications.ToList())
            {
                notification.IsRead = true;
            }
            await context.complete();

            return Json("sucess");

        }
        [HttpGet]
        public IActionResult OnGetSubCities(int GovernorateId)
        {
            var result = context.Cities.GetAllBy(c => c.GovernorateId == GovernorateId).Result;
            return Json(result);
        }
        [HttpGet]
        public async Task<IActionResult> UnverifiedUsers()
        {
            var customers = await context.Customers.GetAllBy(c => c.IsVerified == false
                                                             && c.NationalIdImage!=null
                                                             &&c.PhoneNumber!=null
                                                             &&c.ProfileImage!=null);
            return Json(customers);
        }
        [HttpGet]
        public async Task<IActionResult> RestOfCustomers()
        {
            var customers = await context.Customers.GetAllBy(c => c.IsVerified == false
                                                             &&(c.NationalIdImage == null
                                                             || c.PhoneNumber == null
                                                             || c.ProfileImage == null));
            return Json(customers);
        }
    }
}

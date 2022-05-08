using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Paging;
using Sho8lana.Unit_Of_Work;
using System.IO;

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
        public async Task<IActionResult> UnReadMessages()
        {
            var customerId = userManager.GetUserId(User);
            if (customerId == null)
            {
                return NotFound();
            };
            var MessageCount = await context.ServiceMessages.Count(m => m.IsRead == false && m.ReceiverId == customerId);
            return Json(MessageCount);
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
        public IActionResult OnGetSubCities(string GovernorateName)
        {
            var result = context.Cities.GetAllBy(c => c.Governorate.Governorate_name_en == GovernorateName).Result;
            return Json(result);
        }
        [HttpPost]
        public async Task <IActionResult> UploadImage(IFormFile Pic,int ServiceId,int updateNo)
        {
            var service=await context.Services.GetById(ServiceId);
            var name = service.ServiceId + "-" + service.Title + "-" + Guid.NewGuid()+ ".jpg";
            if (updateNo < service.Medias.Count)
            {
                if (updateNo == 0)
                {
                    name = service.Medias.Skip(updateNo).Take(1).FirstOrDefault().MediaPath;
                }
                else
                {
                    name = service.Medias.Skip(updateNo+1).Take(1).FirstOrDefault().MediaPath;
                }
            }
            var path = "./wwwroot/Images/services/" + name;
            using (var stream = new FileStream(path, FileMode.Create))
            {
                Pic.CopyTo(stream);
            }
            if (updateNo >= service.Medias.Count)
            {
                Media media = new Media()
                {
                    ServiceId = service.ServiceId,
                    MediaPath = name
                };
                //add image to medias table
                context.Medias.Add(media);
            }
            await context.complete();
            var Medias = (await context.Medias.GetAllBy(s => s.ServiceId == ServiceId)).Select(s => new
            {
                mediaId = s.MediaId,
                mediaPath = s.MediaPath,
                ServiceId = s.ServiceId
            });
            return Json(Medias);
        }
        [HttpDelete]
        public async Task <IActionResult>DeleteImage(int mediaId)
        {
            var Media=await context.Medias.GetById(mediaId);
            var serviceid=Media.ServiceId;
            context.Medias.Delete(Media);
            var path = "./wwwroot/Images/services/" + Media.MediaPath;
            System.IO.File.Delete(path);
            await context.complete();
            var Medias=(await context.Medias.GetAllBy(s=>s.ServiceId==serviceid)).Select(s => new
            {
                mediaId = s.MediaId,
                mediaPath = s.MediaPath,
                ServiceId = s.ServiceId
            });
            return Json(Medias);
        }
        [HttpGet]
        public  IActionResult GetImages(int serviceId)
        {
            var medias = context.Medias.GetAllBy(s => s.ServiceId == serviceId).Result.Select(s => new
            {
                mediaId = s.MediaId,
                mediaPath = s.MediaPath,
                ServiceId = s.ServiceId
            });
            return Json(medias);
        }
    }
}

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Sho8lana.DTOs;
using Sho8lana.Models;
using Sho8lana.Models.ViewModels;
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
        public async Task<IActionResult> ReadMessage(int  id)
        {
            var message = await context.ServiceMessages.GetById(id);
            if (message != null)
            {
                message.IsRead = true;
                await context.complete();
            }
            
            return Json("success");
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
        public async Task<IActionResult> UploadImage(IFormFile Pic, int ServiceId, int updateNo)
        {
            var service = await context.Services.GetEagerLodingAsync(s => s.ServiceId == ServiceId, new string[] { "Medias" });
            var name = service.ServiceId + "-" + service.Title + "-" + Guid.NewGuid() + ".jpg";
            if (updateNo < service.Medias.Count)
            {
                if (updateNo == 0)
                {
                    name = service.Medias.Skip(updateNo).Take(1).FirstOrDefault().MediaPath;
                }
                else
                {
                    name = service.Medias.Skip(updateNo + 1).Take(1).FirstOrDefault().MediaPath;
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
        public async Task<IActionResult> DeleteImage(int mediaId)
        {
            var Media = await context.Medias.GetById(mediaId);
            var serviceid = Media.ServiceId;
            context.Medias.Delete(Media);
            var path = "./wwwroot/Images/services/" + Media.MediaPath;
            System.IO.File.Delete(path);
            await context.complete();
            var Medias = (await context.Medias.GetAllBy(s => s.ServiceId == serviceid)).Select(s => new
            {
                mediaId = s.MediaId,
                mediaPath = s.MediaPath,
                ServiceId = s.ServiceId
            });
            return Json(Medias);
        }
        [HttpGet]
        public IActionResult GetImages(int serviceId)
        {
            var medias = context.Medias.GetAllBy(s => s.ServiceId == serviceId).Result.Select(s => new
            {
                mediaId = s.MediaId,
                mediaPath = s.MediaPath,
                ServiceId = s.ServiceId
            });
            return Json(medias);
        }
        [HttpGet]
        public async Task<IActionResult> Search(string Searchtext,bool? Type,bool? Trusted,int? CategoryId,int? ServiceRate,int? ServicePrice,int pg=1)
        {
            ServiceSearchProperties serviceSearchProperties = new()
            {
                Searchtext = Searchtext,
                Type = Type,
                Trusted = Trusted,
                CategoryId = CategoryId,
                ServiceRate = ServiceRate,
                ServicePrice = ServicePrice==-1?null:ServicePrice,

            };

            const int PageSize = 16;

            var ServicesCount = (await context.Services.Search(serviceSearchProperties)).Count();

            var pager = new pagination(ServicesCount, pg, PageSize);

            int rescPage = (pg - 1) * PageSize;

            serviceSearchProperties.Take = PageSize;

            serviceSearchProperties.Skip = rescPage;

            var Service = await context.Services.Search(serviceSearchProperties);

            var model = Service.Select(s => new ServiceDisplay
            {
                Title = s.Title,
                ServiceId = s.ServiceId,
                ServiceHeader = s.Medias.FirstOrDefault()==null?null: s.Medias.FirstOrDefault().MediaPath,
                Rate = s.Rate,
                Price = s.Price,
                IsFreelance = s.IsFreelancer,
                IsCash = s.IsCash,
                CustomerImage = s.Customer.ProfilePicture,
                CreatedDate = s.PublishDate.ToShortDateString(),
                customerId = s.CustomerId,
                BuyersCount = s.Contracts.Where(s => s.IsDone == true).Count()
            }).ToList();

            var Viewmodel = new { model = model, pager = pager };

            return Json(Viewmodel);

        }

        [HttpGet]
        public async Task<IActionResult> OldSearch(string text, string type, string Truste, int? CatId, int? Rate, int? Price, int pg = 1)
        {
            IEnumerable<Service> services = new List<Service>();
            if (text == null)
            {
                services = await context.Services
                    .GetAllEagerLodingAsync(service => service.IsAccepted
                    && service.IsFreelancer == (type == "services" ? true : false)
                    && service.Rate <= (Rate ?? 10)
                    && service.Price <= (Price ?? int.MaxValue));
            }
            else
            {
                services = await context.Services
                 .GetAllEagerLodingAsync(service => service.IsAccepted
                  && service.IsFreelancer == (type == "services" ? true : false)
                  && service.Rate <= (Rate ?? 10)
                  && service.Price <= (Price ?? int.MaxValue)
                  && (service.Description.Contains(text) || service.Title.Contains(text)));

            }
            if (!(CatId is null))
                services = services.Where(s => s.CategoryId == CatId);

            if (!(Truste is null))
                services = services.Where(s => s.Customer.IsVerified == Convert.ToBoolean(Truste));

            if (Rate == null && Price == null)
                services = services.OrderByDescending(s => s.Rate);

            else if (Rate == null)
                services = services.OrderByDescending(s => s.Price);

            else
                services = services.OrderByDescending(s => s.Rate).OrderByDescending(s => s.Price);

            if (services == null)
                return NotFound();

            //////paging section
            const int PageSize = 16;
            int RecsCount = services.Count();
            var pager = new pagination(RecsCount, pg, PageSize);
            int rescPage = (pg - 1) * PageSize;
            var data = services.Skip(rescPage).Take(pager.PageSize).ToList();
            //////

            var model = data.Select(s => new ServiceDisplay
            {
                Title = s.Title,
                ServiceId = s.ServiceId,
                ServiceHeader = s.Medias.FirstOrDefault()?.MediaPath,
                Rate = s.Rate,
                Price = s.Price,
                IsFreelance = s.IsFreelancer,
                IsCash = s.IsCash,
                CustomerImage = s.Customer.ProfilePicture,
                CreatedDate = s.PublishDate.ToShortDateString(),
                customerId = s.CustomerId,
                BuyersCount = s.Contracts.Where(s => s.IsDone == true).Count()
            }).ToList();
            var Viewmodel = new { model = model, pager = pager };
            return Json(Viewmodel);
        }
        public async Task<IActionResult> Index(int? id, string type, int pg = 1)
        {
            IEnumerable<Service> services = new List<Service>();
            const int PageSize = 12;
            pagination pager;
            int servicesCount;
            if (id == null)
            {
                servicesCount = await context.Services
                    .Count(s => s.IsFreelancer == (type == "services" ? true : false)
                    && s.IsAccepted);

                pager = new pagination(servicesCount, pg, PageSize);
                int rescPage = (pg - 1) * PageSize;

                services = await context.Services
                    .GetAllEagerLodingAsync(s => s.IsFreelancer == (type == "services" ? true : false)
                    && s.IsAccepted,
                    rescPage,pager.PageSize);
            }

            else
            {
                servicesCount = await context.Services.Count(s => s.IsFreelancer == (type == "services" ? true : false)
                && s.IsAccepted
                && s.CategoryId == id
                );

                pager = new pagination(servicesCount, pg, PageSize);
                int rescPage = (pg - 1) * PageSize;

                services = await context.Services
                     .GetAllEagerLodingAsync(s => s.IsFreelancer == (type == "services" ? true : false)
                     && s.IsAccepted
                     && s.CategoryId == id,
                     rescPage,pager.PageSize);
            }
            if(services==null)
                return NotFound();
            
            var model = services.Select(s => new ServiceDisplay
            {
                Title = s.Title,
                ServiceId = s.ServiceId,
                ServiceHeader = s.Medias.FirstOrDefault()?.MediaPath,
                Rate = s.Rate,
                Price = s.Price,
                IsFreelance = s.IsFreelancer,
                IsCash = s.IsCash,
                CustomerImage = s.Customer.ProfilePicture,
                CreatedDate = s.PublishDate.ToShortDateString(),
                customerId = s.CustomerId,
                BuyersCount = s.Contracts.Where(s => s.IsDone == true).Count(),
            }).ToList();

            var Viewmodel = new {model=model,pager=pager};

            return Json(Viewmodel);

        }
        [AcceptVerbs("GET", "POST")]
        public async Task<IActionResult> ConfirmUniquePhoneNumber([Bind(Prefix = "Input.PhoneNumber")] string phoneNumber)
        {
            bool NotExists=(await context.Customers.GetBy(s=>s.PhoneNumber==phoneNumber)==null)?true:false;
            return Json(NotExists);

        }

        public async Task<IActionResult> ConfirmUniquePhoneNumberEditing([Bind(Prefix = "Input.PhoneNumber")] string phoneNumber, [Bind(Prefix = "Input.Userid")] string Userid)
        {
            var user= await userManager.FindByIdAsync(Userid);
            bool NotExists=false;
            if (await context.Customers.GetBy(s => s.PhoneNumber == phoneNumber) == null)
            {
                NotExists = true;
            }
            else if(user.PhoneNumber==phoneNumber)
            {
                NotExists=true;
            }
            return Json(NotExists);
        }

        public async Task<IActionResult> ConfirmUniqueEmail([Bind(Prefix = "Input.Email")] string email)
        {
            bool NotExists = (await context.Customers.GetBy(s => s.Email == email) == null) ? true : false;
            return Json(NotExists);

        }       
        public async Task<IActionResult> ConfirmUniqueEmaillogin([Bind(Prefix = "Input.Email")] string email)
        {
            bool NotExists = (await context.Customers.GetBy(s => s.Email == email) == null) ? false : true;

            return Json(NotExists);

        }
    }
}

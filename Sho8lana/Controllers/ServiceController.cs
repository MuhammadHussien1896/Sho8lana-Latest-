using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Data;
using Sho8lana.Models;
using Sho8lana.Unit_Of_Work;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Sho8lana.Models.ViewModels;
using Microsoft.AspNetCore.Authorization;

namespace Sho8lana.Controllers
{
    public class ServiceController : Controller
    {
        string [] supportedTypes = new[] { "jpg", "jpeg", "png", "gif", "JPG", "JPEG", "PNG", "GIF" };
        

        private readonly IUnitOfWork _context ;
        private readonly UserManager<Customer> userManager;

        public ServiceController(IUnitOfWork context,UserManager<Customer> userManager)
        {
            _context = context;
            this.userManager = userManager;
        }

        /*
        // GET: Service
        public async Task<IActionResult> Index()
        {
            
            var applicationDbContext = _context.Services.Include(s => s.Category).Include(s => s.Customer);
            return View(await applicationDbContext.ToListAsync());
        }
        */
        
        // GET: Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var service = await _context.Services.GetById(id);

            if(service == null)
            {
                return NotFound();
            }

            return View("Details_modified",service);


        }
        // GET: Service/Create
        [Authorize(Roles ="User")]
        public IActionResult Create()
        {
            var categories =_context.Categories.GetAllSync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
                       
            return View();
        }

        // POST: Service/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [Authorize(Roles = "User")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,Description,Title,Price,CustomerInstructions,IsCash,IsFreelancer,PublishDate,Rate,CategoryId,CustomerId")] Service service,List<IFormFile> Medias)
        {
            int i = 1;
            if (ModelState.IsValid)
            {
               
                // publish date
                service.PublishDate = DateTime.Now;

                //Customer Id 
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var customerId = claims.Value;
                
                service.CustomerId = customerId;
                //add service to database
                if (Medias.Count > 4)
                {
                    return View(service);
                }
                else
                {
                    var s = _context.Services.Add(service);
                    await _context.complete();

                    //loop for every image in media list 
                    foreach (var image in Medias)
                    {
                        var name = s.ServiceId + "-" + s.Title + "-" + Guid.NewGuid() + ".jpg";
                        var path = "./wwwroot/Images/services/" + name;

                        using (var stream = new FileStream(path, FileMode.Create))
                        {
                            image.CopyTo(stream);
                            Media media = new Media()
                            {
                                ServiceId = s.ServiceId,
                                MediaPath = name
                            };
                            //add image to medias table
                            _context.Medias.Add(media);
                            //await _context.complete();

                        }
                        i++;
                    }
                    await _context.complete();
                    return RedirectToAction("Details",new {id = s.ServiceId});
                }
                ////////////////////////////////////////////////////////////
                //return RedirectToAction(nameof(Index));
            }
            var categories = _context.Categories.GetAllSync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", service.CategoryId);
            
            return View(service);
        }
       // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customerId = userManager.GetUserId(User);
            var service = await _context.Services.GetById(id);
            if (customerId != service.CustomerId)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            if (id == 0) { return NotFound(); }
            
            if(service.CustomerRequests.Count > 0 || service.Contracts.Count > 0)
            {
                return RedirectToAction(nameof(Details),new { id = id});
            }
            _context.Services.Delete(service);
            await _context.complete();
            return RedirectToAction("Index", "Categories");

        }

        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var customerId = userManager.GetUserId(User);
            var service = await _context.Services.GetById(id);
            if(id == null || service == null)
            {
                return NotFound();
            }
            if (customerId != service.CustomerId)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            var categories = _context.Categories.GetAllSync();
            //if (id == null)
            //{
            //    return NotFound();
            //}

            
            //if (service == null)
            //{
            //    return NotFound();
            //}
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", service.CategoryId);
            return View(service);
        }

        // POST: Service/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,Description,Title,Price,CustomerInstructions,IsCash,IsFreelancer,PublishDate,Rate,CategoryId,CustomerId")] Service service,List<IFormFile> Medias)
        {
            //int i = 1;
            var categories = _context.Categories.GetAllSync();
            var customerId = userManager.GetUserId(User);
            if (customerId != service.CustomerId)
            {
                return LocalRedirect("~/Identity/Account/AccessDenied");
            }
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    service.IsAccepted = false;
                    _context.Services.Update(service);
                    Notification notification = new Notification
                    {
                        Content = $"تم تعديل خدمة {service.Title} وهى الان في مرحلة المراجعة ",
                        Date = DateTime.Now,
                        IsRead = false,
                        CustomerId = service.CustomerId,
                    };
                     _context.Notifications.Add(notification);
                    await _context.complete();

                    //var allImages = _context.Medias.GetAllBy(m=>m.ServiceId==service.ServiceId);
                    //foreach(var image in await allImages) 
                    //{
                    //    await _context.Medias.Delete(image.MediaId);
                    //    await _context.complete();
                    //}
                    ////loop for every image in media list 
                    //foreach (var image in Medias)
                    //{

                    //    var path = "./wwwroot/Images/services/" + service.ServiceId + "-" + service.Title + "-" + i + ".jpg";

                    //    using (var stream = new FileStream(path, FileMode.Create))
                    //    {
                    //        image.CopyTo(stream);
                    //        Media media = new Media()
                    //        {
                    //            ServiceId = service.ServiceId,
                    //            MediaPath = service.ServiceId + "-" + service.Title + "-" + i + ".jpg"
                    //        };
                    //        //add image to medias table
                    //        _context.Medias.Add(media);
                    //        await _context.complete();

                    //    }
                    //    i++;
                    //}
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (await ServiceExists(service.ServiceId)==false)
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction("Index", "Categories");
            }
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", service.CategoryId);
            return View(service);
        }

        
        
        private async Task <bool> ServiceExists(int id)
        {
            var service =  await _context.Services.GetBy(e => e.ServiceId == id);
            if(service == null) { return true; }
            else { return false; }
        }


        public async Task<IActionResult> SendMessage(int id)
        {
            var service = await _context.Services.GetBy(e => e.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }
        public IActionResult DisplayMessages()
        {
            var id = userManager.GetUserId(User);
            if(id == null)
            {
                return NotFound();
            }
            var allMessages = _context.ServiceMessages
                                .GetAllBy(m => m.CustomerId == id || m.ReceiverId == id).Result.OrderByDescending(m => m.MessageDate)
                                .GroupBy(m => m.ServiceId);
            var latestMessages = new List<ServiceMessage>();
            foreach (var item in allMessages)
            {
                latestMessages.Add(item.FirstOrDefault());
            }
            return View(latestMessages);
        }
        public async Task<IActionResult> Chat(int id,string receiverId)
        {
            var userId = userManager.GetUserId(User);
            var receiver = await _context.Customers.GetBy(c => c.Id == receiverId);
            var service = await _context.Services.GetBy(s => s.ServiceId == id);
            if (userId == null || receiver == null || service == null)
            {
                return NotFound();
            }
            var chatMessages = _context.ServiceMessages
                .GetAllBy(m => ((m.CustomerId == userId && m.ReceiverId == receiverId) 
                || (m.CustomerId == receiverId && m.ReceiverId == userId))
                && m.ServiceId == id).Result.OrderByDescending(m => m.MessageDate);
            var receiverName = $"{receiver.FirstName} {receiver.LastName}";
            var onlineReceiver = await _context.OnlineUsers.GetBy(u => u.UserId == receiverId);
            ChatViewModel model = new ChatViewModel()
            {
                SenderId = userId,
                ReceiverName = receiverName,
                ReceiverId = receiverId,
                Messages = chatMessages,
                ServiceTitle = service.Title,
                ServiceId = id,
                IsReceiverOnline = onlineReceiver != null
            };
            var unreadMessages = await _context.ServiceMessages.GetAllBy(m => m.ReceiverId == userId && m.ServiceId == id && m.IsRead == false);
            foreach(var messsage in unreadMessages)
            {
                messsage.IsRead = true;
            }
            _context.ServiceMessages.UpdateList(unreadMessages.ToList());
            await _context.complete();
            return View(model);
            
        }
    }
}

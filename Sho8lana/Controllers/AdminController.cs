using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Sho8lana.Hangfire;
using Sho8lana.Hubs;
using Sho8lana.Models;
using Sho8lana.Models.ViewModels;
using Sho8lana.Paging;
using Sho8lana.Unit_Of_Work;

namespace Sho8lana.Controllers
{
    [Authorize(Roles ="Admin")]
    public class AdminController : Controller
    {
        private readonly IUnitOfWork _context;

        private readonly UserManager<Customer> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ContractJobs jobs;

        public AdminController(IUnitOfWork context, UserManager<Customer> userManager, RoleManager<IdentityRole> roleManager, IHubContext<ChatHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            jobs = new ContractJobs(context, hubContext);
        }
        
        
        public async Task<IActionResult>AdminPanel()
        {
            var numberOfCustommers = await _context.Customers.Count();
            ViewBag.NumberOfCustommers = numberOfCustommers;

            var numOfVerifiedUsers = await _context.Customers.Count(a => a.IsVerified);
            ViewBag.numOfVerifiedUsers = numOfVerifiedUsers;

            var numOfUnverifiedUsers = await _context.Customers.Count(c => c.IsVerified == false);
            ViewBag.numOfUnverifiedUsers = numOfUnverifiedUsers;

            var numberOfServices = await _context.Services.Count();
            ViewBag.numberOfServices = numberOfServices;

            var numOfDoneContracts = await _context.Contracts.Count(c=>c.IsDone == true);
            ViewBag.numOfDoneContracts = numOfDoneContracts;

            var numOfOnlineContracts = await _context.Contracts.Count(c => c.IsDone == false && c.SellerAccepted == true && c.BuyerAccepted == true);
            ViewBag.numOfOnlineContracts = numOfOnlineContracts;

            return View();
        }

        /*Review Roles*/

        public async Task<IActionResult> ReviewRolesToUser()
        {
            //var roles =await _userManager.GetRolesAsync();
            var users =  _userManager.Users.Select(user => new UserViewModel
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                Roles = _userManager.GetRolesAsync(user).Result
            }).ToList();
            return View(users);
        }
        public async Task<IActionResult> ManageRoles(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();
            var roles = await _roleManager.Roles.ToListAsync();

            var viewModel = new UserRolesViewModel
            {
                UserId = user.Id,
                UserName = user.UserName,
                Roles = roles.Select(role => new RoleViewModel
                {
                    RoleId = role.Id,
                    RoleName = role.Name,
                    IsSelected = _userManager.IsInRoleAsync(user, role.Name).Result
                }).ToList()
            };
            return View(viewModel);
        }
        [HttpPost]
        public async Task<IActionResult> ManageRoles(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);
            if (user == null)
                return NotFound();
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in model.Roles)
            {
                if (userRoles.Any(r => r == role.RoleName) && !role.IsSelected)
                    await _userManager.RemoveFromRoleAsync(user, role.RoleName);
                if (!userRoles.Any(r => r == role.RoleName) && role.IsSelected)
                    await _userManager.AddToRoleAsync(user, role.RoleName);
            }
            return RedirectToAction(nameof(ReviewRolesToUser));

        }

        public async Task<IActionResult> ReviewUsers(string type,int pg=1)
        {
            IEnumerable<Customer> customers = new List<Customer>();
            if (type == null)
            {
                customers = await _context.Customers.GetAllBy(c => c.IsVerified == true);
                type = "Verfied";
            }
            if (type == "Verfied")
            {
                customers = await _context.Customers.GetAllBy(c => c.IsVerified == true);
            }

            else if (type == "Unverfied")
            {
                customers = await _context.Customers.GetAllBy(c => c.IsVerified == false
                                                && c.NationalIdPicture != null
                                                && c.PhoneNumberConfirmed==true
                                                && c.ProfilePicture != null);
            }
            else if (type == "Rest")
            {
                 customers = await _context.Customers.GetAllBy(c => c.IsVerified == false
                                                 && (c.NationalIdPicture == null
                                                 || c.PhoneNumber == null
                                                 || c.ProfilePicture == null));
            }

            //////paging section
            const int PageSize = 16;
            int RecsCount = customers.Count();
            var pager = new pagination(RecsCount, pg, PageSize);
            int rescPage = (pg - 1) * PageSize;
            var data = customers.Skip(rescPage).Take(pager.PageSize).ToList();
            ViewBag.pager = pager;
            //////
            
            ViewBag.Type = type;
            
            return View(data);
        }

        public async Task<IActionResult> ReviewServices()
        {
            var services = _context.Services
                .GetAllEagerLodingAsync(a => a.IsAccepted == false&&a.IsRejected==false
                ,new string[] {"Category","Customer"}).Result;

            return View(services);
        }

       
        [HttpPost]
        public async Task<IActionResult> UpdateListToActive(List<Service> services)
        {
            try
            {
                _context.Services.UpdateList(services);
                 //_context.Complete1();
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


                    //user.IsVerified = true;
                    await jobs.AddNotification(user.Id, "تهانينا تمت  مراجعت الخدمة واضافتها بنجاح  ");
                    //Notification notification = new Notification()
                    //{
                    //    CustomerId = user.Id,
                    //    Content = "تهانينا تمت  مراجعت الخدمة واضافتها بنجاح  ",
                    //    Date = DateTime.Now,
                    //};
                    //_context.Notifications.Add(notification);
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
                    if (messagenotfi == null) { messagenotfi =$"خدمة {service.Title} لا تطابق السياسة الخاصة بموقع شغلانة نرجو تعديل تلك الخدمة مرة اخرى "; }
                    //user.IsVerified = true;
                    await jobs.AddNotification(user.Id, $"رفض لخدمة{service.Title} : "+messagenotfi);
                    //Notification notification = new Notification()
                    //{
                    //    CustomerId = user.Id,
                    //    Content = messagenotfi,
                    //    Date = DateTime.Now,
                    //};
                    //_context.Notifications.Add(notification);
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
                await jobs.AddNotification(user.Id, "تهانينا , لقد تم التحقق من هويتك الشخصية ");
                //Notification notification = new Notification()
                //{
                //    CustomerId = user.Id,
                //    Content = "تهانينا , لقد تم التحقق من هويتك الشخصية ",
                //    Date = DateTime.Now,
                //};
                //_context.Notifications.Add(notification);
                await _context.complete();

                return RedirectToAction("ReviewUsers");
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
                await jobs.AddNotification(user.Id, "معذرة , لقد تم تعذر التحقق من هويتك الشخصية , حاول مرة اخري");
                //Notification notification = new Notification()
                //{
                //    CustomerId = user.Id,
                //    Content = "معذرة , لقد تم تعذر التحقق من هويتك الشخصية , حاول مرة اخري",
                //    Date = DateTime.Now,
                //};
                //_context.Notifications.Add(notification);
                await _context.complete();

                return RedirectToAction("ReviewUsers");
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



        public async Task<IActionResult> ShowCategories()
        {
            var categories = await _context.Categories.GetAll();
 
            return View(categories);

        }

        public IActionResult CreateCategory()
        {
            //var categories = await _context.Categories.GetAll();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(CreateCategoryViewModel category)
        {
            if (ModelState.IsValid)
            {
                using (FileStream fs = new FileStream("./wwwroot/Images/CategoriesImage/"+ category.CategoryImg.FileName, FileMode.Create))
                {
                    category.CategoryImg.CopyTo(fs);
                    Category category1 = new Category()
                    {
                        Name = category.Name,
                        Description = category.Description,
                        CategoryImg = category.CategoryImg.FileName
                    };

                    _context.Categories.Add(category1);
                    await _context.complete();

                }
                return RedirectToAction(nameof(ShowCategories));
            }
            return View(category);
        }
        
        public async Task<IActionResult> ShowServiceMessages(int id)//complain id
        {
            var complain = await _context.Complains.GetEagerLodingAsync(c => c.ComplainId == id,new string[] {"Contract"});
            var contract = complain?.Contract;
            if (contract == null)
            {
                return NotFound();
            }
            var messages = _context.ServiceMessages
                .GetAllBy(m => ((m.CustomerId == contract.BuyerId && m.ReceiverId == contract.SellerId)
                || (m.CustomerId == contract.SellerId && m.ReceiverId == contract.BuyerId))
                && m.ServiceId == contract.ServiceId).Result;
            var buyer = await _context.Customers.GetById(contract.BuyerId);
            var seller = await _context.Customers.GetById(contract.SellerId);
            ShowMessagesViewModel model = new ShowMessagesViewModel()
            {
                Messages    = messages.ToList(),
                BuyerName   = buyer.FirstName +" "+ buyer.LastName,
                SellerName  = seller.FirstName + " " + seller.LastName,
                BuyerId     = contract.BuyerId,
                SellerId    = contract.SellerId,
                ServiceId   = contract.ServiceId
            };

            return View(model);
        }
        public async Task<IActionResult> ReturnPriceToBuyer(int id)//contract id
        {
            var contract = await _context.Contracts.GetEagerLodingAsync(c => c.ComplainId == id,new string[] { "Complain","Service" });
            var buyer = await _context.Customers.GetById(contract?.BuyerId);
            var seller = await _context.Customers.GetById(contract?.SellerId);
            if(contract == null || buyer == null || seller == null)
            {
                return NotFound();
            }
            //compare between now and contract end date + 14 (for seller pending balance)
            var dateCompare = DateTime.Compare(DateTime.Now, contract.EndDate.AddDays(14));
            //if now is earlier we are ok to return the contract price
            if(dateCompare < 0)
            {
                //return price from seller pending to buyer balance
                seller.PendingBalance -= contract.ContractPrice;
                buyer.Balance += contract.ContractPrice;
                //_context.Customers.UpdateList(new List<Customer>() { seller, buyer });
                contract.Complain.IsReturned = true;
                await jobs.AddNotification(contract.BuyerId
                    , $" لقد تمت مراجعة الشكوى التي قدمتها وتبين أحقيتك بها ، وتم ارجاع سعر الخدمة {contract.ContractPrice}$ لك ");
                await jobs.AddNotification(contract.SellerId, $"تم ارجاع سعر الخدمة '{contract.Service.Title}' للبائع نظرا لإرسال شكوى بخصوص الخدمة" +
                    $"وتبين أحقيته في استرجاع سعر الخدمة");
                await _context.complete();
                return RedirectToAction("ShowComplains");
            }
            else
            {
                return Content("To late :(");
            }
            
            
        }

        [HttpGet]
        public async Task<IActionResult> ShowComplains()
        {
            var Complains=await _context.Complains.GetAllEagerLodingAsync(c=>c.IsSolved==false,new string[] {"Contract.Service.Category","Contract.Customer", "Contract.Service.Customer" });
            return View(Complains);
        }
        
        public async Task<IActionResult> ReplayAdminToComplaint(int id,string message=null)
        {
            var complaint = await _context.Complains.GetEagerLodingAsync(c => c.ComplainId == id, new string[] { "Contract" });
            string userAdminId = _userManager.GetUserId(User);
            var userAdmin=await _userManager.FindByIdAsync(userAdminId);

            if (complaint == null) { return NotFound(); }
            else
            {
                try
                {
                    complaint.AdminReply = message == null ? $"تم مراجعة الشكوي بواسطة {userAdmin.UserName} " : message+$"لقك تم الرد بواسطة {userAdmin.UserName}وتم الرد ب : ";
                    var userid = complaint.Contract.BuyerId;
                    var user = _context.Customers.GetById(userid);
                    //// add Notifi
                    await jobs.AddNotification(userid,"الرد على الشكوى : "+ message ?? "تم مراجعة الشكوى وسوف نقوم بالاجراء اللازم تجاه تلك العملية ");
                    //Notification notification = new Notification()
                    //{
                    //    CustomerId = user.id,
                    //    Content = message == null ? "تم مراجعة الشكوى وسوف نقوم بالاجراء اللازم تجاه تلك العملية " : message,
                    //    Date = DateTime.Now,
                    //};
                    //_context.Notifications.Add(notification);
                    await _context.complete();

                    return RedirectToAction(nameof(ShowComplains));
                }
                catch
                {
                    
                    return RedirectToAction(nameof(ShowComplains));

                }
            }
        }

        public async Task<IActionResult> ReviewComplaintAndSolved(int id)
        {
            var complaint = await _context.Complains.GetEagerLodingAsync(c => c.ComplainId == id,new string[] {"Contract.Service"});
            if (complaint == null) { return NotFound(); }
            try {

                complaint.IsSolved = true;
                var userid = complaint.Contract.BuyerId;
                await jobs.AddNotification(userid
                    , $" إذا كان لديك اي شكوى اخرى يرجى الاتصال بالدعم الفنى لموقع شغلانة  {complaint.Contract.Service.Title} تهانينا لقد تم مراجعة الشكوى المقدمة منكم بخصوص الخدمة  ");
                //Notification notification = new Notification()
                //{
                //    CustomerId = user.Id,
                //    Content = $" إذا كان لديك اي شكوى اخرى يرجى الاتصال بالدعم الفنى لموقع شغلانة  {complaint.Contract.Service.Title} تهانينا لقد تم مراجعة الشكوى المقدمة منكم بخصوص الخدمة  ",
                //    Date = DateTime.Now,
                //};
                //_context.Notifications.Add(notification);
                await _context.complete();

                return RedirectToAction(nameof(ShowComplains));

            } catch
            {
                    return View();

            }
        }
    }
}











//                var path = "./wwwroot/assets/img/services/" + s.ServiceId + "-" + s.Title + "-" + i + ".jpg";

//                using (var stream = new FileStream(path, FileMode.Create))
//                {
//                    image.CopyTo(stream);
//                    Media media = new Media()
//                    {
//                        ServiceId = s.ServiceId,
//                        MediaPath = s.ServiceId + "-" + s.Title + "-" + i + ".jpg"
//                    };
//                    //add image to medias table
//                    _context.Medias.Add(media);
//                    await _context.complete();

//                }
//                i++;
//            }
//        }
//        ////////////////////////////////////////////////////////////
//        return RedirectToAction(nameof(Index));
//    }
//    var categories = _context.Categories.GetAllSync();
//    ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name", service.CategoryId);

//    return View(service);
//}








﻿using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
       
        public AdminController(IUnitOfWork context, UserManager<Customer> userManager, RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
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
            var users = _userManager.Users.Select(user => new UserViewModel
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
                                                && c.PhoneNumber != null
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
            const int PageSize = 2;
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
                Notification notification = new Notification()
                {
                    CustomerId = user.Id,
                    Content = "معذرة , لقد تم تعذر التحقق من هويتك الشخصية , حاول مرة اخري",
                    Date = DateTime.Now,
                };
                _context.Notifications.Add(notification);
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

        public async Task<IActionResult> CreateCategory()
        {
            var categories = await _context.Categories.GetAll();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory([Bind("Name,Description")] Category category, IFormFile CategoryImg)
        {
            if (ModelState.IsValid)
            {
                using (FileStream fs = new FileStream("./wwwroot/Images/CategoriesImage/"+ CategoryImg.FileName, FileMode.Create))
                {
                    CategoryImg.CopyTo(fs);
                    Category category1 = new Category()
                    {
                        Name = category.Name,
                        Description = category.Description,
                        CategoryImg = CategoryImg.FileName
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
            var complain = await _context.Complains.GetEagerLodingAsync(c => c.ContractId == id,new string[] {"Contract"});
            var contract = complain?.Contract;
            if (contract == null)
            {
                return NotFound();
            }
            var messages = _context.ServiceMessages
                .GetAllBy(m => ((m.CustomerId == contract.BuyerId && m.ReceiverId == contract.SellerId)
                || (m.CustomerId == contract.SellerId && m.ReceiverId == contract.BuyerId))
                && m.ServiceId == contract.ServiceId).Result.OrderByDescending(m => m.MessageDate);
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
            var contract = await _context.Contracts.GetEagerLodingAsync(c => c.ContractId == id,new string[] { "Complain" });
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
                await _context.complete();
                return RedirectToAction("ShowComplains");
            }
            else
            {
                return Content("To late :(");
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








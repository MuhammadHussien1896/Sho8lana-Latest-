using Microsoft.AspNetCore.Mvc;
using Sho8lana.Models;
using Sho8lana.Paging;
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


        public async Task<IActionResult>Index()
        {

            return View();
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
                                                && c.NationalIdImage != null
                                                && c.PhoneNumber != null
                                                && c.ProfileImage != null);
            }
            else if (type == "Rest")
            {
                 customers = await _context.Customers.GetAllBy(c => c.IsVerified == false
                                                 && (c.NationalIdImage == null
                                                 || c.PhoneNumber == null
                                                 || c.ProfileImage == null));
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
            var services = _context.Services.GetAllBy(a=>a.IsAccepted==false).Result;

            return View(services);
        }

        public async Task<IActionResult> ShowDetails(int id)
        {
            string[] nameOfIncludes =new string[6] {"Category", "Customer", "Contracts", "CustomerRequests", "ServiceMessages", "Medias" };
            var serviceDetails = _context.Services.GetEagerLodingAsync(a => a.ServiceId == id,nameOfIncludes).Result;
            if (serviceDetails == null) return NotFound();
            return View(serviceDetails);
        }
        [HttpPost]
        public async Task<IActionResult> ShowDetails(Service service)
        {
            try {
                _context.Services.Update(service);
               await _context.complete();
                return RedirectToAction(nameof(ReviewServices));
            } catch {
                return View();
            }
        }
        [HttpPost]
        public async Task<IActionResult> UpdateListToActive(List<Service> services)
        {
            try {
                _context.Services.UpdateList(services);
                await _context.complete();
                return RedirectToAction(nameof(ReviewServices));

            } catch
            {
                return NotFound();
            }
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








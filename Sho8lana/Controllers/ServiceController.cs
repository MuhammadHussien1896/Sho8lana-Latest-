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

namespace Sho8lana.Controllers
{
    public class ServiceController : Controller
    {
        
       
        private readonly IUnitOfWork _context ;

        public ServiceController(IUnitOfWork context)
        {
            _context = context;
            
        }
        
        /*
        // GET: Service
        public async Task<IActionResult> Index()
        {
            
            var applicationDbContext = _context.Services.Include(s => s.Category).Include(s => s.Customer);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }
        */
        // GET: Service/Create
        public IActionResult Create()
        {
            var categories =_context.Categories.GetAllSync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "Name");
                       
            return View();
        }

        // POST: Service/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("ServiceId,Description,Title,Price,CustomerInstructions,IsCash,IsFreelancer,PublishDate,Rate,CategoryId,CustomerId")] Service service,List<IFormFile> Medias)
        {
            if (ModelState.IsValid)
            {
                //IMAGE
                foreach(var image in Medias)
                {
                    using(var stream = new FileStream("./wwwroot/assets/img/services/" + image.FileName, FileMode.Create))
                    {
                        image.CopyTo(stream);
                    }
                }
                
            ///////////////////////////////////////////////////////
            
                // publish date
                service.PublishDate = DateTime.Now;

                //Customer Id 
                var claimsIdentity = (ClaimsIdentity)User.Identity;
                var claims = claimsIdentity.FindFirst(ClaimTypes.NameIdentifier);
                var customerId = claims.Value;
                
                service.CustomerId = customerId;

                _context.Services.Add(service);
                //await _context.SaveChangesAsync();
                await _context.complete();
                return RedirectToAction(nameof(Index));
            }
            var categories = _context.Categories.GetAllSync();
            ViewData["CategoryId"] = new SelectList(categories, "CategoryId", "CategoryId", service.CategoryId);
            
            return View(service);
        }
        /*
        // GET: Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", service.CategoryId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", service.CustomerId);
            return View(service);
        }

        // POST: Service/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,Description,Title,Price,CustomerInstructions,IsCash,IsFreelancer,PublishDate,Rate,CategoryId,CustomerId")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(service);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["CategoryId"] = new SelectList(_context.Categories, "CategoryId", "CategoryId", service.CategoryId);
            ViewData["CustomerId"] = new SelectList(_context.Customers, "Id", "Id", service.CustomerId);
            return View(service);
        }

        // GET: Service/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .Include(s => s.Category)
                .Include(s => s.Customer)
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var service = await _context.Services.FindAsync(id);
            _context.Services.Remove(service);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }*/
    }
}

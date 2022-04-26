﻿using Microsoft.AspNetCore.Mvc;
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
       
    }
}

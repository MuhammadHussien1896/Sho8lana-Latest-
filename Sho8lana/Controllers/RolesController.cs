using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Sho8lana.Controllers
{

    [Authorize(Roles = "Admin")]
    public class RolesController : Controller
    {
        private readonly RoleManager<IdentityRole> roleManager;
        public RolesController(RoleManager<IdentityRole> _roleManager)
        {
            roleManager = _roleManager;
        }
        public async Task<IActionResult> Index()
        {
            return View(await roleManager.Roles.ToListAsync());
        }
        [HttpPost]
        public async Task<IActionResult> Add(IdentityRole model)
        {
            if (!ModelState.IsValid)
                return View("Index", await roleManager.Roles.ToListAsync());
            var roleIsExists = await roleManager.RoleExistsAsync(model.Name);
            if (roleIsExists)
            {
                ModelState.AddModelError("Name", "Role Is Exists");
                return View("Index", await roleManager.Roles.ToListAsync());
            }
            await roleManager.CreateAsync(new IdentityRole(model.Name.Trim()));
            return RedirectToAction(nameof(Index));

        }
    }

}

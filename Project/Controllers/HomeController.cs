using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Gruppe4NLA.Controllers
{
    [Authorize] 
    public class HomeController : Controller 
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<ApplicationUser> _userManager; 
        private readonly RoleManager<IdentityRole> _roleManager;

        public HomeController(
            ILogger<HomeController> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Pilot start view (map)
        [Authorize(Roles = "Pilot")]
        public IActionResult Leaflet() => View();

        [HttpGet]
        [AllowAnonymous] 
        public IActionResult Index()
        {
            // Not signed in: go to login
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(Index), "Home");
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
            }

            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Adminpage), "Home");

            if (User.IsInRole("CaseworkerAdm"))
                return RedirectToAction("Inbox", "Reports");

            if (User.IsInRole("Caseworker"))
                return RedirectToAction("Inbox", "Reports");

            if (User.IsInRole("Pilot"))
                return RedirectToAction(nameof(Leaflet), "Home");

            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        // Map view – allow any of these roles
        [Authorize(Roles = "Pilot,Caseworker,CaseworkerAdm,Admin")]
        public IActionResult Map() => View("Leaflet");

        [AllowAnonymous]
        public IActionResult FAQ() => View();

        [AllowAnonymous]
        public IActionResult LogIn() => RedirectToPage("/Account/Login", new { area = "Identity" });

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Adminpage()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.Email)
                .ToListAsync();

            ViewBag.AllRoles = await _roleManager.Roles
           .Select(r => r.Name!) 
           .OrderBy(n => n) 
           .ToListAsync(); 

            return View("AdminUsers/Adminpage", users); 
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous] 
        public IActionResult Error() 
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

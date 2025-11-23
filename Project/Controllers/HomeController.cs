using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Gruppe4NLA.Controllers
{
    [Authorize] // Default: require auth; open specific actions with [AllowAnonymous]
    public class HomeController : Controller // Class that inherits from Controller
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

        // Role-based router for "/"
        [HttpGet]
        [AllowAnonymous] // unauthenticated users allowed
        public IActionResult Index()
        {
            // Not signed in: go to login
            if (!(User?.Identity?.IsAuthenticated ?? false))
            {
                var returnUrl = Url.Action(nameof(Index), "Home");
                return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
            }

            // Signed in: route by role (priority order)
            if (User.IsInRole("Admin"))
                return RedirectToAction(nameof(Adminpage), "Home");

            if (User.IsInRole("CaseworkerAdm"))
                return RedirectToAction("Inbox", "Reports");

            if (User.IsInRole("Caseworker"))
                return RedirectToAction("Inbox", "Reports");

            if (User.IsInRole("Pilot"))
                return RedirectToAction(nameof(Leaflet), "Home");

            // No mapped role
            return RedirectToPage("/Account/AccessDenied", new { area = "Identity" });
        }

        // Map view – allow any of these roles
        [Authorize(Roles = "Pilot,Caseworker,CaseworkerAdm,Admin")]
        public IActionResult Map() => View("Leaflet");

        [AllowAnonymous]
        public IActionResult FAQ() => View();

        [AllowAnonymous]
        public IActionResult LogIn() => RedirectToPage("/Account/Login", new { area = "Identity" });

        // Admin user management view
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Adminpage()
        {
            // Load all users and pass them as the model
            var users = await _userManager.Users
                .OrderBy(u => u.Email) // "u" represents each user
                .ToListAsync();

            ViewBag.AllRoles = await _roleManager.Roles
           .Select(r => r.Name!) // "r" represents each role
           .OrderBy(n => n) // "n" represents each role name
           .ToListAsync(); // List of all role names

            return View("AdminUsers/Adminpage", users); //
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous] // Allow unauthenticated users
        public IActionResult Error() // Error view
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;

namespace Gruppe4NLA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Leaflet()
        {
            return View();
        }


        public IActionResult Index()
        {
            if (User.Identity?.IsAuthenticated == true)
                return RedirectToAction(nameof(Leaflet), "Home");

            var returnUrl = Url.Action(nameof(Leaflet), "Home");
            return RedirectToPage("/Account/Login", new { area = "Identity", returnUrl });
        }

        [AllowAnonymous]
        public IActionResult Privacy()
        {
            return View();
        }

        [AllowAnonymous]
        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }
        
        [AllowAnonymous]
        public IActionResult About()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Administrator()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

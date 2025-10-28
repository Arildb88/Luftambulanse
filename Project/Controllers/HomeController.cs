using System.Diagnostics;
using Gruppe4NLA.Models;
using Gruppe4NLA.Models.ViewModels; // For ViewModel
using Gruppe4NLA.DataContext; // For ApplicationDbContext       
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore; // For DbContext (for ToListAsync, where, OrderBy, etc.)

namespace Gruppe4NLA.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationContext _context; // DbContext for database access                                

        // added ApplicationDbContext to constructor for database access (aplplicaiondbcontext to constructor for querying the database)
        public HomeController(ILogger<HomeController> logger, ApplicationContext context)
        {
            _logger = logger;
            _context = context;
        }

        [HttpGet("/")]
        public IActionResult Leaflet()
        {
            return View();
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult FAQ()
        {
            return View();
        }

        public IActionResult LogIn()
        {
            return View();
        }
        //public IActionResult SignIn()
        //{
        //    return View();
        //}
        
        public IActionResult About()
        {
            return View();
        }
        public IActionResult Administrator()
        {
            return View();
        }

        // New Reports action method to fetch and display reports
        public async Task<IActionResult> Reports()
        {
            var drafts = await _context.Reports
                .Where(r => r.Status == ReportStatus.Draft)
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();          

            var submitted = await _context.Reports
                .Where(r => r.Status == ReportStatus.Submitted)
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();          

            var vm = new ReportsOverviewVm
            {
                Drafts = drafts,
                Submitted = submitted
            };                           

            return View(vm);             // return View with ViewModel
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}

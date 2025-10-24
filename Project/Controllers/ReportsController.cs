using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Gruppe4NLA.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public ReportsController(AppDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            IQueryable<ReportModel> q = _context.Reports;

            // Non-admin/caseworker: only see own reports
            if (!(User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm")))
            {
                var myId = _userManager.GetUserId(User);
                var me = await _userManager.GetUserAsync(User);
                var email = me?.Email;

                q = q.Where(r => r.UserId == myId
                              || (r.UserId == null && r.SenderName == email)); // fallback for old rows
            }

            var reports = await q.OrderByDescending(r => r.DateSent).ToListAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportModelWrapper model)
        {
            var user = await _userManager.GetUserAsync(User);

            model.NewReport.SenderName = user?.Email;

            // Gammel kode, kan slettes
            //model.NewReport.SenderName = user?.UserName ?? User.Identity?.Name;

            //// SenderName isn't posted anymore, so remove stale ModelState entry
            ModelState.Remove("NewReport.SenderName");

            if (!ModelState.IsValid)
            {
                model.SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync();

                return View(model);
            }

            var newReport = new ReportModel
            {
                UserId = model.NewReport.UserId,
                Latitude = model.NewReport.Latitude,
                Longitude = model.NewReport.Longitude,
                SenderName = model.NewReport.SenderName,
                DangerType = model.NewReport.DangerType,
                Details = model.NewReport.Details,
                HeightInnMeters = model.NewReport.HeightInnMeters,
                AreLighted = model.NewReport.AreLighted,
                DateSent = DateTime.Now
            };

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            model.NewReport = new ReportModel(); // Reset input
            model.SubmittedReport = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();

            ViewBag.Message = "Submitted successfully!";
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create(double? lat, double? lng)
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = _userManager.GetUserId(User),
                    SenderName = user?.Email,
                },
                
                SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };

            if (lat.HasValue)
                model.NewReport.Latitude = lat.Value;

            if (lng.HasValue)
                model.NewReport.Longitude = lng.Value;

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpGet("/Reports/CreateFromMap")]
        public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
        {
            var user = await _userManager.GetUserAsync(User);            
            
            var vm = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = _userManager.GetUserId(User),
                    SenderName = user?.Email,          // set the sender name from form
                    Latitude = lat ?? default,
                    Longitude = lng ?? default
                },
                SubmittedReport = await _context.Reports
            .OrderByDescending(r => r.DateSent)
            .ToListAsync()
            };

            return View("Create", vm);
        }
    }
}




// Commented out
//[HttpGet("/Reports/CreateFromMap")]
//public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
//{
//    var user = await _userManager.GetUserAsync(User);

//    var vm = new ReportModelWrapper
//    {
//        NewReport = new ReportModel(),
//        SubmittedReport = await _context.Reports
//            .OrderByDescending(r => r.DateSent)
//            .ToListAsync()
//    };

//    if (lat.HasValue) vm.NewReport.Latitude = lat.Value;
//    if (lng.HasValue) vm.NewReport.Longitude = lng.Value;

//    return View("Create", vm);
//}





// Commented out original code:

//using Gruppe4NLA.DataContext;

//using Gruppe4NLA.Models;
//using Microsoft.AspNetCore.Mvc;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Microsoft.EntityFrameworkCore;



//namespace Gruppe4NLA.Controllers
//{
//    public class ReportsController : Controller
//    {
//        private readonly AppDbContext _context;

//        public ReportsController(AppDbContext context)
//        {
//            _context = context;
//        }

//        public async Task<IActionResult> Index()
//        {
//            var reports = await _context.Reports
//                .OrderByDescending(r => r.DateSent)
//                .ToListAsync();

//            return View(reports);
//        }

//        [HttpPost]
//        public async Task<IActionResult> Create(ReportModelWrapper model)
//        {
//            if (!ModelState.IsValid)
//            {
//                model.SubmittedReport = await _context.Reports
//                    .OrderByDescending(r => r.DateSent)
//                    .ToListAsync();

//                return View(model);
//            }

//            var newReport = new ReportModel
//            {
//                Latitude = model.NewReport.Latitude,
//                Longitude = model.NewReport.Longitude,
//                SenderName = model.NewReport.SenderName,
//                DangerType = model.NewReport.DangerType,
//                Details = model.NewReport.Details,
//                HeightInnMeters = model.NewReport.HeightInnMeters, // change variable names for the report form? -jonas
//                AreLighted = model.NewReport.AreLighted,
//                DateSent = DateTime.Now
//            };

//            _context.Reports.Add(newReport);
//            await _context.SaveChangesAsync();

//            model.NewReport = new ReportModel(); // Reset input
//            model.SubmittedReport = await _context.Reports
//                .OrderByDescending(r => r.DateSent)
//                .ToListAsync();

//            ViewBag.Message = "Submitted successfully!";
//            return View(model);
//        }


//        [HttpGet]
//        public async Task<IActionResult> Create(double? lat, double? lng)
//        {
//            var model = new ReportModelWrapper
//            {
//                NewReport = new ReportModel(),
//                SubmittedReport = await _context.Reports
//                    .OrderByDescending(r => r.DateSent)
//                    .ToListAsync()
//            };

//            if (lat.HasValue)
//                model.NewReport.Latitude = lat.Value;

//            if (lng.HasValue)
//                model.NewReport.Longitude = lng.Value;

//            return View(model);
//        }        

//        // Show the "details" page
//        public async Task<IActionResult> Details(int id)
//        {
//            var report = await _context.Reports.FindAsync(id);
//            if (report == null)
//            {
//                return NotFound();
//            }

//            return View(report);
//        }

//        // GET /Reports/CreateFromMap?lat=..&lng=..
//        [HttpGet("/Reports/CreateFromMap")]
//        public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
//        {
//            var vm = new ReportModelWrapper
//            {
//                NewReport = new ReportModel(),
//                SubmittedReport = await _context.Reports
//                    .OrderByDescending(r => r.DateSent)
//                    .ToListAsync()
//            };

//            if (lat.HasValue) vm.NewReport.Latitude = lat.Value;
//            if (lng.HasValue) vm.NewReport.Longitude = lng.Value;

//            // Reuse the same Create view
//            return View("Create", vm);
//        }

//    }
//}


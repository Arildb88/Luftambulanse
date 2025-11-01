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
            // If user is Admin go straight to view all reports, if not sort by your username
            if (!(User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm") || User.IsInRole("Caseworker")))
            {
                var myId = _userManager.GetUserId(User);
                var me = await _userManager.GetUserAsync(User);
                var email = me?.Email;

                // Checks myId = Identity UserId, Fallback where old rows sorted by email
                q = q.Where(r => r.UserId == myId
                              || (r.UserId == null && r.SenderName == email));
            }

            var reports = await q.OrderByDescending(r => r.DateSent).ToListAsync();
            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportModelWrapper model)
        {
            var user = await _userManager.GetUserAsync(User);

            model.NewReport.SenderName = user?.Email;

            ModelState.Remove("NewReport.SenderName");

            // Validate OtherDangerType if "Other" is selected
            if (model.NewReport.Type == ReportModel.DangerType.Other
                && string.IsNullOrWhiteSpace(model.NewReport.OtherDangerType))
            {
                ModelState.AddModelError(nameof(model.NewReport.OtherDangerType), "Please describe the obstacle.");
            }

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
                GeoJson = model.NewReport.GeoJson,
                SenderName = model.NewReport.SenderName,
                Type = model.NewReport.Type,
                OtherDangerType = model.NewReport.OtherDangerType,
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

        [HttpPost]
        public async Task<IActionResult> CreatePopUp(ReportModelWrapper model)
        
        {
            // Validate OtherDangerType if "Other" is selected
            if (model.NewReport.Type == ReportModel.DangerType.Other
                && string.IsNullOrWhiteSpace(model.NewReport.OtherDangerType))
            {
                ModelState.AddModelError(nameof(model.NewReport.OtherDangerType), "Please describe the obstacle.");
            }

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
                GeoJson = model.NewReport.GeoJson,
                SenderName = model.NewReport.SenderName,
                Type = model.NewReport.Type,
                OtherDangerType = model.NewReport.OtherDangerType,
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

        [HttpGet]
        public async Task<IActionResult> CreatePopUp(double? lat, double? lng)
        {
            var user = await _userManager.GetUserAsync(User);

            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = _userManager.GetUserId(User),
                    SenderName = user?.Email  // fills in the user's email as sender name
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

        // Gets the userid from the _usermanager, puts the senderName as user's Email.
        [HttpGet("/Reports/CreateFromMap")]
        public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
        {
            var user = await _userManager.GetUserAsync(User);            
            
            var vm = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = _userManager.GetUserId(User), // hidden but its used for selecting witch reports you can view in Reports View
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

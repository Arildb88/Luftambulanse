using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IActionResult> Index(string? filter)
        {
            IQueryable<ReportModel> q = _context.Reports;

            // Non-admin/caseworker: only see own reports
            if (!(User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm") || User.IsInRole("Caseworker")))
            {
                var myId = _userManager.GetUserId(User);
                var me = await _userManager.GetUserAsync(User);
                var email = me?.Email;

                // Either same Identity UserId, or legacy rows matched by email
                q = q.Where(r => r.UserId == myId
                              || (r.UserId == null && r.SenderName == email));
            }

            var f = (filter ?? "all").ToLowerInvariant();
            if (f == "submitted")
                q = q.Where(r => r.Status == ReportStatus.Submitted);
            else if (f == "drafts")
                q = q.Where(r => r.Status == ReportStatus.Draft);

            ViewData["Filter"] = f;

            var reports = await q.OrderByDescending(r => r.DateSent).ToListAsync();
            return View(reports);
        }

        // === CreatePopUp (POST) – save/submit from the map popup, stay on same view and show green message ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePopUp(ReportModelWrapper model, string? action)
        {
            // Validate "Other"
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
                HeightInMeters = model.NewReport.HeightInMeters,
                AreLighted = model.NewReport.AreLighted,
                DateSent = DateTime.Now
            };

            // Status from button
            if (string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase))
            {
                newReport.Status = ReportStatus.Submitted;
            }
            else
            {
                newReport.Status = ReportStatus.Draft;
            }

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            // Green message on the same page
            ViewBag.Message = newReport.Status == ReportStatus.Submitted
                ? "Submit was successful"
                : "Draft was successful";

            // Keep these fields in the form after save
            var keepLat = model.NewReport.Latitude;
            var keepLng = model.NewReport.Longitude;
            var keepSender = model.NewReport.SenderName;

            // Refresh list if shown below
            model.SubmittedReport = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();

            // Reset inputs but keep key fields
            model.NewReport = new ReportModel
            {
                Latitude = keepLat,
                Longitude = keepLng,
                SenderName = keepSender
            };

            ModelState.Clear();
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

        // Create from map (route alias)
        [HttpGet("/Reports/CreateFromMap")]
        public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
        {
            var user = await _userManager.GetUserAsync(User);

            var vm = new ReportModelWrapper
            {
                NewReport = new ReportModel
                {
                    UserId = _userManager.GetUserId(User),
                    SenderName = user?.Email,
                    Latitude = lat ?? default,
                    Longitude = lng ?? default
                },
                SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };

            return View("Create", vm);
        }

        // === Draft feature ===
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.Status == ReportStatus.Submitted)
            {
                if (report.Status != ReportStatus.Draft)
                    return RedirectToAction(nameof(Details), new { id });
            }

            return View(report);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportModel updated, string? action)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.Status == ReportStatus.Submitted)
            {
                TempData["Error"] = "Cannot edit a submitted report.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid)
                return View(updated);

            // Update editable fields
            report.Latitude = updated.Latitude;
            report.Longitude = updated.Longitude;
            report.GeoJson = updated.GeoJson;
            report.Type = updated.Type;
            report.OtherDangerType = updated.OtherDangerType;
            report.Details = updated.Details;
            report.HeightInMeters = updated.HeightInMeters;
            report.AreLighted = updated.AreLighted;

            if (string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase))
            {
                report.Status = ReportStatus.Submitted;
                
            }
            else
            {
                report.Status = ReportStatus.Draft;
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = (report.Status == ReportStatus.Submitted)
                ? "Report submitted."
                : "Draft updated.";

            return RedirectToAction(nameof(Index), new { filter = (report.Status == ReportStatus.Draft) ? "drafts" : "submitted" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.Status == ReportStatus.Submitted)
            {
                TempData["Message"] = "Already submitted.";
                return RedirectToAction(nameof(Index), new { filter = "submitted" });
            }

            report.Status = ReportStatus.Submitted;
            
            await _context.SaveChangesAsync();

            TempData["Message"] = "Report submitted.";
            return RedirectToAction(nameof(Index), new { filter = "submitted" });
        }
        // === /Draft feature ===
    }
}

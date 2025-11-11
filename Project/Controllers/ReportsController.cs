using System;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Gruppe4NLA.ViewModels;

using InboxRowVM = Gruppe4NLA.ViewModels.ReportListItemVM;

namespace Gruppe4NLA.Controllers
{
    // Added AutoValidateAntiforgeryToken globally to all controllers who needs it, beter than forgetting to add later
    // You can still override per-controller or per-action if needed.
    [AutoValidateAntiforgeryToken]
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;


        private readonly IReportAssignmentService _assigner;

        public ReportsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IReportAssignmentService assigner
        )
        {
            _context = context;
            _userManager = userManager;
            _assigner = assigner;
        }

        public async Task<IActionResult> Index(string? filter, string rsort = "DateSent", string rdir = "desc")
        {
            IQueryable<ReportModel> q = _context.Reports;

            // Visibility as you had
            if (!(User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm") || User.IsInRole("Caseworker")))
            {
                var myId = _userManager.GetUserId(User);
                var me = await _userManager.GetUserAsync(User);
                var email = me?.Email;
                q = q.Where(r => r.UserId == myId || (r.UserId == null && r.SenderName == email));
            }

            // Tabs
            var f = (filter ?? "all").ToLowerInvariant();
            if (f == "submitted") q = q.Where(r => r.Status == ReportStatus.Submitted);
            else if (f == "drafts") q = q.Where(r => r.Status == ReportStatus.Draft);

            // Normalize dir
            rdir = (rdir?.ToLower() == "asc") ? "asc" : "desc";

            // Reports-specific sorting
            switch ((rsort ?? "").ToLowerInvariant())
            {
                case "sender":
                    q = rdir == "asc" ? q.OrderBy(r => r.SenderName)
                                      : q.OrderByDescending(r => r.SenderName);
                    break;

                case "height":
                    q = rdir == "asc" ? q.OrderBy(r => r.HeightInMeters ?? 0)
                                      : q.OrderByDescending(r => r.HeightInMeters ?? 0);
                    break;

                case "type":
                    q = rdir == "asc" ? q.OrderBy(r => r.Type)
                                      : q.OrderByDescending(r => r.Type);
                    break;

                case "status":
                    // Sort by the StatusCase you display (custom order)
                    q = (rdir == "asc")
                        ? q.OrderBy(r =>
                            r.StatusCase == ReportStatusCase.Submitted ? 0 :
                            r.StatusCase == ReportStatusCase.Assigned ? 1 :
                            r.StatusCase == ReportStatusCase.InReview ? 2 :
                            r.StatusCase == ReportStatusCase.Completed ? 3 : 4)
                        : q.OrderByDescending(r =>
                            r.StatusCase == ReportStatusCase.Submitted ? 0 :
                            r.StatusCase == ReportStatusCase.Assigned ? 1 :
                            r.StatusCase == ReportStatusCase.InReview ? 2 :
                            r.StatusCase == ReportStatusCase.Completed ? 3 : 4);
                    break;

                case "datesent":
                default:
                    q = rdir == "asc" ? q.OrderBy(r => r.DateSent)
                                      : q.OrderByDescending(r => r.DateSent);
                    break;
            }

            var reports = await q.AsNoTracking().ToListAsync();

            ViewData["Filter"] = f;
            ViewBag.RSort = rsort;
            ViewBag.RDir = rdir;

            return View(reports);
        }


        // === CreatePopUp (POST) – save/submit from the map popup, stay on same view and show green message ===
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePopUp(ReportModelWrapper model, string? action)
        {

            // Konverter høyde til meter hvis brukeren har valgt "feet"
            if (model.NewReport.HeightUnit == "feet" && model.NewReport.HeightInMeters.HasValue)
            {
                // 1 foot = 0.3048 meter
                model.NewReport.HeightInMeters = model.NewReport.HeightInMeters * 0.3048;
            }

            if (!ModelState.IsValid)
            {
                model.SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync();
                return View(model);
            }

            double heightMeters = model.NewReport.HeightUnit == "feet"
                ? (model.NewReport.HeightInMeters ?? 0) / 3.28084
                : (model.NewReport.HeightInMeters ?? 0);

            var newReport = new ReportModel
            {
                UserId = model.NewReport.UserId,
                Latitude = model.NewReport.Latitude,
                Longitude = model.NewReport.Longitude,
                GeoJson = model.NewReport.GeoJson,
                SenderName = model.NewReport.SenderName,
                Type = model.NewReport.Type,
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
                    UserId = _userManager.GetUserId(User), // hidden but used to select which reports you can view
                    SenderName = user?.Email               // fills in the user's email as sender name
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

        // Shows detailed information about a specific report
        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            // Load the assigned user's email if report is assigned
            if (!string.IsNullOrEmpty(report.AssignedToUserId))
            {
                var assignedUser = await _userManager.FindByIdAsync(report.AssignedToUserId);
                if (assignedUser != null)
                {
                    // Store the email in ViewBag to display in the view
                    ViewBag.AssignedToEmail = assignedUser.Email ?? assignedUser.UserName ?? report.AssignedToUserId;
                }
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
                    UserId = _userManager.GetUserId(User), // used to filter "my reports"
                    SenderName = user?.Email,              // set the sender name from the user
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
            // (Removed: OtherDangerType update)
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


        // Admin inbox: list all reports with assignment/status info
        [Authorize(Roles = "CaseworkerAdm,Caseworker")]
        public async Task<IActionResult> Inbox()
        {
            var data = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .Select(r => new InboxRowVM
                {
                    Id = r.Id,
                    SenderName = r.SenderName,
                    Type = r.Type.ToString(),
                    DateSent = r.DateSent,
                    Status = r.Status.ToString(),
                    AssignedTo = r.AssignedToUserId != null
                        ? _context.Users
                            .Where(u => u.Id == r.AssignedToUserId)
                            .Select(u => u.Email ?? u.UserName ?? u.Id)
                            .FirstOrDefault()
                        : null,
                    // Gets organization of the user
                    Organization =
                _context.Users
                    .Where(u => u.Id == r.UserId)
                    .Select(u => u.Organization)
                    .FirstOrDefault()
                ?? _context.Users
                    .Where(u => u.Email == r.SenderName)
                    .Select(u => u.Organization)
                    .FirstOrDefault()
                })
                .ToListAsync();

            return View(data);
        }

        // Show assignment dialog to choose a Caseworker
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Assign(int id)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

            // Load users who have the "Caseworker" role
            var caseworkers = await _context.Users
                .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r.Name })
                .Where(x => x.Name == "Caseworker")
                .Select(x => x.u)
                .ToListAsync();

            var vm = new AssignReportVM
            {
                ReportId = report.Id,
                CurrentAssignee = report.AssignedToUserId,
                Caseworkers = caseworkers
                    .Select(u => new SelectListItem
                    {
                        Value = u.Id,
                        Text = u.UserName ?? u.Email ?? u.Id
                    })
                    .ToList()
            };

            return View(vm);
        }

        // Perform the assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Assign(AssignReportVM vm)
        {
            if (!ModelState.IsValid)
            {
                // Repopulate caseworker list if validation fails
                var caseworkers = await _context.Users
                    .Join(_context.UserRoles, u => u.Id, ur => ur.UserId, (u, ur) => new { u, ur })
                    .Join(_context.Roles, x => x.ur.RoleId, r => r.Id, (x, r) => new { x.u, r.Name })
                    .Where(x => x.Name == "Caseworker")
                    .Select(x => x.u)
                    .ToListAsync();

                vm.Caseworkers = caseworkers
                    .Select(u => new SelectListItem { Value = u.Id, Text = u.UserName ?? u.Email ?? u.Id })
                    .ToList();

                return View(vm);
            }

            var me = _userManager.GetUserId(User)!;
            await _assigner.AssignAsync(vm.ReportId, vm.ToUserId!, me);

            TempData["Ok"] = "Report assigned.";
            return RedirectToAction(nameof(Inbox));
        }

        // Remove assignment
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Unassign(int id)
        {
            var me = _userManager.GetUserId(User)!;
            await _assigner.UnassignAsync(id, me);
            TempData["Ok"] = "Assignment removed.";
            return RedirectToAction(nameof(Inbox));
        }

        // Caseworker self-assigns an unassigned report
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        public async Task<IActionResult> SelfAssign(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                await _assigner.SelfAssignAsync(id, userId);
                TempData["Ok"] = "Case successfully assigned to you!";
                return RedirectToAction(nameof(MyQueue));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Inbox));
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Caseworker approves a report
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                await _assigner.ApproveAsync(id, userId);
                TempData["Ok"] = "Report approved successfully";
                return RedirectToAction(nameof(MyQueue));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Caseworker rejects a report
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                await _assigner.RejectAsync(id, userId);
                TempData["Ok"] = "Report rejected";
                return RedirectToAction(nameof(MyQueue));
            }
            catch (InvalidOperationException ex)
            {
                TempData["Error"] = ex.Message;
                return RedirectToAction(nameof(Details), new { id });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        // Caseworker's personal queue: what is assigned to me and in progress
        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        [HttpGet("/MyQueue", Name = "MyQueueRoute")]
        public async Task<IActionResult> MyQueue(string sort = "AssignedAt", string dir = "asc")
        {
            var me = _userManager.GetUserId(User)!;
            dir = (dir?.ToLower() == "desc") ? "desc" : "asc";

            var q = _context.Reports
                .Where(r => r.AssignedToUserId == me &&
                            (r.StatusCase == ReportStatusCase.Assigned || r.StatusCase == ReportStatusCase.InReview))
                .Select(r => new MyQueueItemVM
                {
                    Id = r.Id,
                    SenderName = r.SenderName,
                    Organization =
                        _context.Users.Where(u => u.Id == r.UserId).Select(u => u.Organization).FirstOrDefault()
                        ?? _context.Users.Where(u => u.Email == r.SenderName).Select(u => u.Organization).FirstOrDefault(),
                    Type = r.Type.ToString(),
                    DateSent = r.DateSent,
                    AssignedAtUtc = r.AssignedAtUtc,
                    StatusCase = r.StatusCase
                });

            // (sorting switch here, same as before)

            var items = await q.AsNoTracking().ToListAsync();
            ViewBag.Sort = sort; ViewBag.Dir = dir;
            return View(items);
        }


        public class AssignReportVM
        {
            [Required] public int ReportId { get; set; }
            public string? CurrentAssignee { get; set; }

            [Required(ErrorMessage = "Please select a caseworker")]
            public string? ToUserId { get; set; }

            public List<SelectListItem> Caseworkers { get; set; } = new();
        }

        public class ReportListItemVM
        {
            public int Id { get; set; }
            public string? SenderName { get; set; }
            public string? Organization { get; set; }
            public string? Type { get; set; }
            public DateTime DateSent { get; set; }
            public string Status { get; set; } = "";
            public string? AssignedTo { get; set; }
        }
    }
}

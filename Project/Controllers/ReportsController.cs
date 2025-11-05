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



namespace Gruppe4NLA.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        // NEW: delegation service (assign/unassign logic)
        private readonly IReportAssignmentService _assigner;

        public ReportsController(
            AppDbContext context,
            UserManager<ApplicationUser> userManager,
            IReportAssignmentService assigner // NEW
        )
        {
            _context = context;
            _userManager = userManager;
            _assigner = assigner; // NEW
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
            if (model.NewReport.Type == ReportModel.DangerTypeEnum.Other
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
            if (model.NewReport.Type == ReportModel.DangerTypeEnum.Other
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
        
        
        /// Admin inbox: list all reports with assignment/status info.
        /// Only CaseworkerAdm can open this page.
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Inbox()
        {
            var data = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .Select(r => new ReportListItemVM
                {
                    Id = r.Id,
                    SenderName = r.SenderName,
                    DangerType = r.DangerType,
                    DateSent = r.DateSent,
                    Status = r.Status.ToString(),      // requires ReportStatus on your model
                    AssignedTo = r.AssignedToUserId    // shows Id; can be mapped to username later
                })
                .ToListAsync();

            return View(data);
        }
        
        /// Show assignment dialog to choose a Caseworker.
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
        
        /// Perform the assignment 
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

            var me = _userManager.GetUserId(User)!; // admin performing the assignment
            await _assigner.AssignAsync(vm.ReportId, vm.ToUserId!, me);

            TempData["Ok"] = "Report assigned.";
            return RedirectToAction(nameof(Inbox));
        }
        
        /// Remove assignment 
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
        
        /// Caseworker's personal queue: what is assigned to me and in progress.
        [Authorize(Roles = "Caseworker")]
        public async Task<IActionResult> MyQueue()
        {
            var me = _userManager.GetUserId(User)!;

            var items = await _context.Reports
                .Where(r => r.AssignedToUserId == me &&
                            (r.Status == ReportStatus.Assigned || r.Status == ReportStatus.InReview))
                .OrderBy(r => r.AssignedAtUtc)
                .ToListAsync();

            return View(items);
        }
    }
    
    // NEW: ViewModels (lightweight)
    // Place them here or in a separate file if you prefer.
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
        public string? DangerType { get; set; }
        public DateTime DateSent { get; set; }
        public string Status { get; set; } = "";
        public string? AssignedTo { get; set; }
    }
}

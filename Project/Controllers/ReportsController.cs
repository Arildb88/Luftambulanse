using Gruppe4NLA.Areas.Identity.Data;
using Gruppe4NLA.DataContext;
using Gruppe4NLA.Models;
using Gruppe4NLA.Services;
using Gruppe4NLA.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MySqlConnector;
using System;
using System.ComponentModel.DataAnnotations;
using System.Composition;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
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

            if (!(User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm") || User.IsInRole("Caseworker")))
            {
                var myId = _userManager.GetUserId(User);
                var me = await _userManager.GetUserAsync(User);
                var email = me?.Email;
                q = q.Where(r => r.UserId == myId || (r.UserId == null && r.SenderName == email));
            }
            // ---------------- Sorting Filter ---------------- //
            
            var f = (filter ?? "all").ToLowerInvariant();
            if (f == "submitted") q = q.Where(r => r.StatusCase == ReportStatusCase.Submitted);
            else if (f == "drafts") q = q.Where(r => r.StatusCase == ReportStatusCase.Draft);

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

            return View("~/Views/Home/ReportsView.cshtml", reports);
        }


        // CreatePopUp (POST) – save/submit from the map popup, stay on same view and show green message
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePopUp(ReportModelWrapper model, string? action)
        {
            // Convert height to meters for validation
            model.NewReport.HeightInMeters = MetersFeetConverter.ToMeters(
                model.NewReport.HeightInMeters,
                model.NewReport.HeightUnit
                );


            if (!ModelState.IsValid)
            {
                model.SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync();
                return View(model);
            }

            var isSubmitted = string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase);

            var newReport = new ReportModel
            {
                UserId = model.NewReport.UserId,
                GeoJson = model.NewReport.GeoJson,
                SenderName = model.NewReport.SenderName,
                Type = model.NewReport.Type,
                Details = model.NewReport.Details,
                HeightInMeters = model.NewReport.HeightInMeters,
                AreLighted = model.NewReport.AreLighted,
                DateSent = DateTime.Now
            };

            newReport.StatusCase = isSubmitted
                ? ReportStatusCase.Submitted
                : ReportStatusCase.Draft;

            if (isSubmitted)
            {
                newReport.SubmittedAt = DateTime.UtcNow;
            }

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            var confirmation = new ConfirmationViewModel
            {
                Title = isSubmitted
                    ? "Report Submitted"
                    : "Draft Saved",
                Message = isSubmitted
                    ? "Your report has been submitted successfully."
                    : "Your draft has been saved successfully.",
                RedirectUrl = isSubmitted
                    ? Url.Action("Index", "Home")!
                    : Url.Action("Index", "Reports", new { filter = "drafts" })!
            };

            return View("Confirmation", confirmation);
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
                    SenderName = user?.Email               
                },
                SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };
                       
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

            if (!string.IsNullOrEmpty(report.AssignedToUserId))
            {
                var assignedUser = await _userManager.FindByIdAsync(report.AssignedToUserId);
                if (assignedUser != null)
                {
                    ViewBag.AssignedToEmail = assignedUser.Email ?? assignedUser.UserName ?? report.AssignedToUserId;
                }
            }

            var (lat, lng) = GetFirstCoordinateFromGeoJson(report.GeoJson);
            ViewBag.Latitude = lat;
            ViewBag.Longitude = lng;

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
                    SenderName = user?.Email              
                   
                },
                SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };

            return View("Create", vm);
        }

        // Draft feature
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.StatusCase != ReportStatusCase.Draft)
            {
                return RedirectToAction(nameof(Details), new { id });
            }

            return View(report);
        }


        // Delete Report feature
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, string? returnUrl)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (!CanDelete(report)) return Forbid();

            _context.Reports.Remove(report);
            await _context.SaveChangesAsync();

            TempData["Ok"] = "Report deleted.";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            if (User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm") || User.IsInRole("Caseworker"))
                return RedirectToAction(nameof(Inbox));

            return RedirectToAction(nameof(Index));
        }

        // Who may delete?
        private bool CanDelete(ReportModel r)
        {
            // Admin / CaseworkerAdm can delete anything
            if (User.IsInRole("Admin") || User.IsInRole("CaseworkerAdm"))
                return true;

            // Caseworker can delete Drafts and Submitted
            if (User.IsInRole("Caseworker"))
                return r.StatusCase == ReportStatusCase.Draft || r.StatusCase == ReportStatusCase.Submitted;

            // Pilot: only own Drafts
            var myId = _userManager.GetUserId(User);
            var myName = User?.Identity?.Name; 
            if (User.IsInRole("Pilot") && r.StatusCase == ReportStatusCase.Draft)
                return r.UserId == myId ||
                       (r.UserId == null && string.Equals(r.SenderName, myName, StringComparison.OrdinalIgnoreCase));

            return false;
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, ReportModel updated, string? action)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.StatusCase == ReportStatusCase.Submitted)
            {
                TempData["Error"] = "Cannot edit a submitted report.";
                return RedirectToAction(nameof(Index));
            }

            if (!ModelState.IsValid) return View(updated);

            report.GeoJson = updated.GeoJson;
            report.Type = updated.Type;
            report.Details = updated.Details;
            report.HeightInMeters = updated.HeightInMeters;
            report.AreLighted = updated.AreLighted;

            if (string.Equals(action, "submit", StringComparison.OrdinalIgnoreCase))
            {
                report.StatusCase = ReportStatusCase.Submitted;   
                report.SubmittedAt = DateTime.UtcNow;             
            }
            else
            {
                report.StatusCase = ReportStatusCase.Draft;       
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = report.StatusCase == ReportStatusCase.Submitted
                ? "Report submitted."
                : "Draft updated.";

            return RedirectToAction(nameof(Index),
                new { filter = report.StatusCase == ReportStatusCase.Draft ? "drafts" : "submitted" });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Submit(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null) return NotFound();

            if (report.StatusCase == ReportStatusCase.Submitted)
            {
                TempData["Message"] = "Already submitted.";
                return RedirectToAction(nameof(Index), new { filter = "submitted" });
            }

            report.StatusCase = ReportStatusCase.Submitted;

            await _context.SaveChangesAsync();

            TempData["Message"] = "Report submitted.";
            return RedirectToAction(nameof(Index), new { filter = "submitted" });
        }

        // Admin inbox: list all reports with assignment/status info
        [Authorize(Roles = "CaseworkerAdm,Caseworker")]
        public async Task<IActionResult> Inbox(string isort = "DateSent", string idir = "desc")
        {
            idir = (idir?.ToLower() == "asc") ? "asc" : "desc";

            var q = _context.Reports.Select(r => new ReportListItemVM
            {
                Id = r.Id,
                SenderName = r.SenderName,
                Organization =
                    _context.Users.Where(u => u.Id == r.UserId).Select(u => u.Organization).FirstOrDefault()
                    ?? _context.Users.Where(u => u.Email == r.SenderName).Select(u => u.Organization).FirstOrDefault(),
                Type = r.Type.ToString(),
                DateSent = r.DateSent,
                Status = r.StatusCase.ToString(),
                AssignedTo = r.AssignedToUserId != null
                    ? _context.Users.Where(u => u.Id == r.AssignedToUserId)
                        .Select(u => u.Email ?? u.UserName ?? u.Id).FirstOrDefault()
                    : null
            });

            // Sorting (Inbox-specific: isort/idir)
            switch ((isort ?? "").ToLowerInvariant())
            {
                case "sender":
                    q = idir == "asc" ? q.OrderBy(x => x.SenderName) : q.OrderByDescending(x => x.SenderName);
                    break;

                case "organization":
                    q = idir == "asc" ? q.OrderBy(x => x.Organization) : q.OrderByDescending(x => x.Organization);
                    break;

                case "type":
                    q = idir == "asc" ? q.OrderBy(x => x.Type) : q.OrderByDescending(x => x.Type);
                    break;

                case "status":
                    q = (idir == "asc")
                        ? q.OrderBy(x =>
                              x.Status == "Draft" ? 0 :
                              x.Status == "Submitted" ? 1 :
                              x.Status == "Assigned" ? 2 :
                              x.Status == "InReview" ? 3 :
                              x.Status == "Completed" ? 4 :
                              x.Status == "Rejected" ? 5 : 9)
                        : q.OrderByDescending(x =>
                              x.Status == "Draft" ? 0 :
                              x.Status == "Submitted" ? 1 :
                              x.Status == "Assigned" ? 2 :
                              x.Status == "InReview" ? 3 :
                              x.Status == "Completed" ? 4 :
                              x.Status == "Rejected" ? 5 : 9);
                    break;

                case "assignedto":
                    // ASC: Unassigned (null) first, then assigned alphabetically
                    // DESC: Assigned alphabetically, Unassigned last
                    q = (idir == "asc")
                        ? q.OrderByDescending(x => x.AssignedTo == null)   
                             .ThenBy(x => x.AssignedTo)
                        : q.OrderBy(x => x.AssignedTo == null)             
                             .ThenByDescending(x => x.AssignedTo);
                    break;

                case "datesent":
                default:
                    q = idir == "asc" ? q.OrderBy(x => x.DateSent) : q.OrderByDescending(x => x.DateSent);
                    break;
            }

            var data = await q.AsNoTracking().ToListAsync();
            ViewBag.ISort = isort;
            ViewBag.IDir = idir;
            return View(data);
        }

        // Show assignment dialog to choose a Caseworker
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Assign(int id)
        {
            var report = await _context.Reports.FirstOrDefaultAsync(r => r.Id == id);
            if (report == null) return NotFound();

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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Assign(AssignReportVM vm)
        {
            if (!ModelState.IsValid)
            {
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
            await _assigner.AssignAsync(vm.ReportId, vm.ToUserId!);

            TempData["Ok"] = "Report assigned.";
            return RedirectToAction(nameof(Inbox));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "CaseworkerAdm")]
        public async Task<IActionResult> Unassign(int id)
        {
            var me = _userManager.GetUserId(User)!;
            await _assigner.UnassignAsync(id);
            TempData["Ok"] = "Assignment removed.";
            return RedirectToAction(nameof(Inbox));
        }

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

                await _assigner.ApproveAsync(id);
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

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        public async Task<IActionResult> Reject(int id, string rejectReportReason)
        {
            try
            {
                var userId = _userManager.GetUserId(User);
                if (userId == null)
                    return Unauthorized();

                if (string.IsNullOrWhiteSpace(rejectReportReason))
                {
                    TempData["Error"] = "You must provide a reason when rejecting a report.";
                    return RedirectToAction(nameof(Details), new { id });
                }

                await _assigner.RejectAsync(id, rejectReportReason); 
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

        [Authorize(Roles = "Caseworker,CaseworkerAdm")]
        [HttpGet("/MyQueue", Name = "MyQueueRoute")]
        public async Task<IActionResult> MyQueue(string qsort = "AssignedAt", string qdir = "asc")
        {
            var me = _userManager.GetUserId(User)!;
            qdir = (qdir?.ToLower() == "desc") ? "desc" : "asc";

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

            switch ((qsort ?? "").ToLowerInvariant())
            {
                case "sender":
                    q = qdir == "asc"
                        ? q.OrderBy(x => x.SenderName ?? "")
                        : q.OrderByDescending(x => x.SenderName ?? "");
                    break;

                case "organization":
                    q = qdir == "asc"
                        ? q.OrderBy(x => x.Organization ?? "")
                        : q.OrderByDescending(x => x.Organization ?? "");
                    break;

                case "type":
                    q = qdir == "asc"
                        ? q.OrderBy(x => x.Type)
                        : q.OrderByDescending(x => x.Type);
                    break;

                case "datesent":
                    q = qdir == "asc"
                        ? q.OrderBy(x => x.DateSent)
                        : q.OrderByDescending(x => x.DateSent);
                    break;

                case "assignedat":
                default:
                    q = qdir == "asc"
                        ? q.OrderBy(x => x.AssignedAtUtc ?? DateTime.MaxValue)   
                        : q.OrderByDescending(x => x.AssignedAtUtc ?? DateTime.MinValue);
                    break;
            }

            var items = await q.AsNoTracking().ToListAsync();
            ViewBag.QSort = qsort;
            ViewBag.QDir = qdir;
            return View(items);
        }

        [HttpGet]
        public async Task<IActionResult> FullMap()
        {
            var reports = await _context.Reports
                                        .Where(r => !string.IsNullOrEmpty(r.GeoJson))
                                        .ToListAsync();

            return View(reports);
        }

        // Helper: read first [lat, lng] from GeoJson (Point or LineString)
        private static (double? lat, double? lng) GetFirstCoordinateFromGeoJson(string? geoJson)
        {
            if (string.IsNullOrWhiteSpace(geoJson))
                return (null, null);

            try
            {
                // Some rows may have the JSON stored as a quoted string: "\"{...}\"".
                // Normalize that so we always parse real JSON.
                var jsonToParse = geoJson.Trim();

                if (jsonToParse.StartsWith("\"") && jsonToParse.EndsWith("\""))
                {
                    try
                    {
                        // Try to unescape using System.Text.Json
                        jsonToParse = JsonSerializer.Deserialize<string>(jsonToParse)
                                      ?? jsonToParse.Trim('"');
                    }
                    catch
                    {
                        // Fallback: strip outer quotes
                        jsonToParse = jsonToParse.Trim('"');
                    }
                }

                using var doc = JsonDocument.Parse(jsonToParse);
                var root = doc.RootElement;

                // If there's a "type" property, check what kind of GeoJSON this is
                if (root.TryGetProperty("type", out var typeProp) &&
                    typeProp.ValueKind == JsonValueKind.String)
                {
                    var type = typeProp.GetString();

                    // Case 1: FeatureCollection → take first feature.geometry
                    if (type == "FeatureCollection" &&
                        root.TryGetProperty("features", out var features) &&
                        features.ValueKind == JsonValueKind.Array &&
                        features.GetArrayLength() > 0)
                    {
                        var firstFeature = features[0];
                        if (firstFeature.TryGetProperty("geometry", out var geom) &&
                            geom.TryGetProperty("coordinates", out var coords))
                        {
                            return ExtractLatLngFromCoordinates(geom, coords);
                        }
                    }

                    // Case 2: single Feature → use its geometry
                    if (type == "Feature" &&
                        root.TryGetProperty("geometry", out var geom2) &&
                        geom2.TryGetProperty("coordinates", out var coords2))
                    {
                        return ExtractLatLngFromCoordinates(geom2, coords2);
                    }
                }

                // Case 3: raw geometry object at root (with or without "type")
                if (root.TryGetProperty("coordinates", out var coordsRoot))
                {
                    return ExtractLatLngFromCoordinates(root, coordsRoot);
                }
            }
            catch
            {
                // invalid JSON or unexpected structure → just fall back to nulls
            }

            return (null, null);
        }

        private static (double? lat, double? lng) ExtractLatLngFromCoordinates(JsonElement geom, JsonElement coords)
        {
            // Try to read explicit "type" if present
            string? type = null;
            if (geom.TryGetProperty("type", out var typeProp) &&
                typeProp.ValueKind == JsonValueKind.String)
            {
                type = typeProp.GetString();
            }

            // If it's explicitly a Point: [lon, lat]
            if (type == "Point")
            {
                if (coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() >= 2)
                {
                    double lng = coords[0].GetDouble();
                    double lat = coords[1].GetDouble();
                    return (lat, lng);
                }
            }

            // If it's explicitly a LineString: [[lon, lat], ...] → use the first coordinate
            if (type == "LineString")
            {
                if (coords.ValueKind == JsonValueKind.Array &&
                    coords.GetArrayLength() > 0 &&
                    coords[0].ValueKind == JsonValueKind.Array)
                {
                    var first = coords[0];
                    if (first.GetArrayLength() >= 2)
                    {
                        double lng = first[0].GetDouble();
                        double lat = first[1].GetDouble();
                        return (lat, lng);
                    }
                }
            }

            // 🔹 Fallback if "type" is missing:
            //    - [lon, lat] → treat as Point
            //    - [[lon, lat], ...] → treat as LineString
            if (coords.ValueKind == JsonValueKind.Array && coords.GetArrayLength() > 0)
            {
                var first = coords[0];

                // [lon, lat] → Point
                if (first.ValueKind == JsonValueKind.Number && coords.GetArrayLength() >= 2)
                {
                    double lng = coords[0].GetDouble();
                    double lat = coords[1].GetDouble();
                    return (lat, lng);
                }

                // [[lon, lat], ...] → LineString
                if (first.ValueKind == JsonValueKind.Array && first.GetArrayLength() >= 2)
                {
                    double lng = first[0].GetDouble();
                    double lat = first[1].GetDouble();
                    return (lat, lng);
                }
            }

            return (null, null);
        }
    }
}



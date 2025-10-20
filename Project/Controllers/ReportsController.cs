using Gruppe4NLA.DataContext;

using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;



namespace Gruppe4NLA.Controllers
{
    public class ReportsController : Controller
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var reports = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();

            return View(reports);
        }

        [HttpPost]
        public async Task<IActionResult> Create(ReportModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                model.SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync();

                return View(model);
            }

            var newReport = new ReportModel
            {
                Latitude = model.NewReport.Latitude,
                Longitude = model.NewReport.Longitude,
                SenderName = model.NewReport.SenderName,
                DangerType = model.NewReport.DangerType,
                Details = model.NewReport.Details,
                HeightInnMeters = model.NewReport.HeightInnMeters, // change variable names for the report form? -jonas
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
            var model = new ReportModelWrapper
            {
                NewReport = new ReportModel(),
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
        
        // Show the "details" page
        public async Task<IActionResult> Details(int id)
        {
            var report = await _context.Reports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        // GET /Reports/CreateFromMap?lat=..&lng=..
        [HttpGet("/Reports/CreateFromMap")]
        public async Task<IActionResult> CreateFromMap(double? lat, double? lng)
        {
            var vm = new ReportModelWrapper
            {
                NewReport = new ReportModel(),
                SubmittedReport = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };

            if (lat.HasValue) vm.NewReport.Latitude = lat.Value;
            if (lng.HasValue) vm.NewReport.Longitude = lng.Value;

            // Reuse the same Create view
            return View("Create", vm);
        }

    }
}

// Temporary reports (REPLACE LATER WITH DB)

/*

private static readonly List<ReportModel> _sample = new List<ReportModel>

        {
            new ReportModel
            {
                Id = 1,
                SenderName = "Thomas Nilsen",
                Latitude = 59.9139,
                Longitude = 10.7522,
                DangerType = "Big tall pole",
                Details = "This pole is about 70 meters tall.",
                DateSent = DateTime.Parse("2025-09-20 14:12")
            },
            new ReportModel
            {
                Id = 2,
                SenderName = "Tor M Hammeren",
                Latitude = 58.9221,
                Longitude = 6.8954,
                DangerType = "Electricity line",
                Details = "The electricity line is in the middle of nowhere, just 2 poles connecting to nothing.",
                DateSent = DateTime.Parse("2025-09-23 22:30")
            },
            new ReportModel
            {
                Id = 3,
                SenderName = "Peder Tangstad",
                Latitude = 63.4047,
                Longitude = 10.2465,
                DangerType = "Treehouse",
                Details = "There is a treehouse with an antenna on top and some wires around it.",
                DateSent = DateTime.Parse("2025-07-28 12:12")
            }
        };

        // GET: /Reports
        /*public IActionResult Index()
        {
            var reports = _sample.OrderByDescending(r => r.DateSent).ToList();
            return View(reports);
        }

        // GET: /Reports/Details/5
        public IActionResult Details(int id)
        {
            var report = _sample.FirstOrDefault(r => r.Id == id);
            if (report == null)
            {
                return NotFound();
            }

            return View(report);
        }

        [HttpGet]
        public IActionResult Create()
        {
            var model = new ReportModelWrapper
            {
                SubmittedReport = _sample
            };
            return View(model);
        }


        [HttpPost]
        public IActionResult Create(ReportModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                model.SubmittedReport = _sample;
                return View(model);
            }

            int nextId = _sample.Any() ? _sample.Max(r => r.Id) + 1 : 1;

            // Save valid coordinate
            _sample.Add(new ReportModel
            {
                Latitude = model.NewReport.Latitude,
                Longitude = model.NewReport.Longitude,
                SenderName = model.NewReport.SenderName,
                DangerType = model.NewReport.DangerType,
                Details = model.NewReport.Details,
                Id = nextId,
                DateSent = DateTime.Now
            });
            model.NewReport = new ReportModel(); // Reset input
            model.SubmittedReport = _sample;

            ViewBag.Message = "Submitted successfully!";
            return View(model);
        }

    }
}

*/
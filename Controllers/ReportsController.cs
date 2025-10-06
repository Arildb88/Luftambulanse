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
        private readonly ApplicationContext _context;

        public ReportsController(ApplicationContext context)
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
                model.SubmittedCoordinates = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync();

                return View(model);
            }

            var newReport = new ReportModel
            {
                Latitude = model.NewCoordinate.Latitude,
                Longitude = model.NewCoordinate.Longitude,
                SenderName = model.NewCoordinate.SenderName,
                DangerType = model.NewCoordinate.DangerType,
                Details = model.NewCoordinate.Details,
                DateSent = DateTime.Now
            };

            _context.Reports.Add(newReport);
            await _context.SaveChangesAsync();

            model.NewCoordinate = new ReportModel(); // Reset input
            model.SubmittedCoordinates = await _context.Reports
                .OrderByDescending(r => r.DateSent)
                .ToListAsync();

            ViewBag.Message = "Submitted successfully!";
            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var model = new ReportModelWrapper
            {
                SubmittedCoordinates = await _context.Reports
                    .OrderByDescending(r => r.DateSent)
                    .ToListAsync()
            };
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
    }
}

// Add Create, Details, etc. here as needed



// Fake temporary reports (REPLACE LATER WITH DB)

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
                SubmittedCoordinates = _sample
            };
            return View(model);
        }


        [HttpPost]
        public IActionResult Create(ReportModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                model.SubmittedCoordinates = _sample;
                return View(model);
            }

            int nextId = _sample.Any() ? _sample.Max(r => r.Id) + 1 : 1;

            // Save valid coordinate
            _sample.Add(new ReportModel
            {
                Latitude = model.NewCoordinate.Latitude,
                Longitude = model.NewCoordinate.Longitude,
                SenderName = model.NewCoordinate.SenderName,
                DangerType = model.NewCoordinate.DangerType,
                Details = model.NewCoordinate.Details,
                Id = nextId,
                DateSent = DateTime.Now
            });
            model.NewCoordinate = new ReportModel(); // Reset input
            model.SubmittedCoordinates = _sample;

            ViewBag.Message = "Submitted successfully!";
            return View(model);
        }

        ////Arild test prøvde å hente inn det Einar hadde gjort, bedre å lage eget, men kjappere å teste Einar sitt SKAL SLETTES
        //// GET /Reports/CreateFromMap  (unique URL)
        //[HttpGet("/Reports/CreateFromMap")]
        //public IActionResult CreateFromMap(double? lat, double? lng)
        //{
        //    var vm = new ReportModelWrapper { NewCoordinate = new ReportModel() };
        //    if (lat.HasValue) vm.NewCoordinate.Latitude = lat.Value;
        //    if (lng.HasValue) vm.NewCoordinate.Longitude = lng.Value;

        //    return View("Create", vm); // reuse the same Create view
        //}

    }
}

*/
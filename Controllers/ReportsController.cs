using Microsoft.AspNetCore.Mvc;
using Gruppe4NLA.Models;

using System;
using System.Collections.Generic;
using System.Linq;

namespace Gruppe4NLA.Controllers
{
    public class ReportsController : Controller
    {
        // Fake temporary reports (REPLACE LATER WITH DB)
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
        public IActionResult Index()
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

    }
}

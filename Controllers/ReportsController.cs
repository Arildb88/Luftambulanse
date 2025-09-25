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
        private static readonly List<Report> _sample = new List<Report>
        {
            new Report
            {
                Id = 1,
                SenderName = "Thomas Nilsen",
                Latitude = 59.9139,
                Longitude = 10.7522,
                DangerType = "Big tall pole",
                Details = "This pole is about 70 meters tall.",
                DateSent = DateTime.Parse("2025-09-20 14:12")
            },
            new Report
            {
                Id = 2,
                SenderName = "Tor M Hammeren",
                Latitude = 58.9221,
                Longitude = 6.8954,
                DangerType = "Electricity line",
                Details = "The electricity line is in the middle of nowhere, just 2 poles connecting to nothing.",
                DateSent = DateTime.Parse("2025-09-23 22:30")
            },
            new Report
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
        public IActionResult Create()
        {
            return View();
        }
    }
}

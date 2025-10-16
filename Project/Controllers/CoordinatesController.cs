
// Einar

/* using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gruppe4NLA.Controllers
{
    public class CoordinatesController : Controller
    {
        // For demo, keep list in memory
        private static List<ReportModel> _SubmittedReport = new List<ReportModel>();

        [HttpGet]
        public IActionResult CoordinatesTest()
        {
            var model = new ReportModelWrapper
            {
                SubmittedReport = _SubmittedReport
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CoordinatesTest(ReportModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                model.SubmittedReport = _SubmittedReport;
                return View(model);
            }

            // Save valid coordinate
            _SubmittedReport.Add(new ReportModel
            {
                Latitude = model.NewReport.Latitude,
                Longitude = model.NewReport.Longitude
            });

            model.NewReport = new ReportModel(); // Reset input
            model.SubmittedReport = _SubmittedReport;

            ViewBag.Message = "Coordinate submitted successfully!";
            return View(model);
        }
    }
}

*/
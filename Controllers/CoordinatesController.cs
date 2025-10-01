
// Einar

/* using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gruppe4NLA.Controllers
{
    public class CoordinatesController : Controller
    {
        // For demo, keep list in memory
        private static List<ReportModel> _submittedCoordinates = new List<ReportModel>();

        [HttpGet]
        public IActionResult CoordinatesTest()
        {
            var model = new ReportModelWrapper
            {
                SubmittedCoordinates = _submittedCoordinates
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CoordinatesTest(ReportModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                model.SubmittedCoordinates = _submittedCoordinates;
                return View(model);
            }

            // Save valid coordinate
            _submittedCoordinates.Add(new ReportModel
            {
                Latitude = model.NewCoordinate.Latitude,
                Longitude = model.NewCoordinate.Longitude
            });

            model.NewCoordinate = new ReportModel(); // Reset input
            model.SubmittedCoordinates = _submittedCoordinates;

            ViewBag.Message = "Coordinate submitted successfully!";
            return View(model);
        }
    }
}

*/

// Einar

using Gruppe4NLA.Models;
using Microsoft.AspNetCore.Mvc;

namespace Gruppe4NLA.Controllers
{
    public class CoordinatesController : Controller
    {
        // For demo, keep list in memory
        private static List<CoordinatesViewModel> _submittedCoordinates = new List<CoordinatesViewModel>();

        [HttpGet]
        public IActionResult CoordinatesTest()
        {
            var model = new CoordinatesViewModelWrapper
            {
                SubmittedCoordinates = _submittedCoordinates
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult CoordinatesTest(CoordinatesViewModelWrapper model)
        {
            if (!ModelState.IsValid)
            {
                // Return view with validation errors
                model.SubmittedCoordinates = _submittedCoordinates;
                return View(model);
            }

            // Save valid coordinate
            _submittedCoordinates.Add(new CoordinatesViewModel
            {
                Latitude = model.NewCoordinate.Latitude,
                Longitude = model.NewCoordinate.Longitude
            });

            model.NewCoordinate = new CoordinatesViewModel(); // Reset input
            model.SubmittedCoordinates = _submittedCoordinates;

            ViewBag.Message = "Coordinate submitted successfully!";
            return View(model);
        }
    }
}

/*using Gruppe4NLA.DataContext;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;

namespace Gruppe4NLA.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GeoJsonController : Controller
    {
        private readonly AppDbContext _context;

        public GeoJsonController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetGeoJsonData")]
        public async Task<IActionResult> GetGeoJsonData()
        {
            // Assuming Reports table has a property called GeoJson
            var reports = await _context.Reports
                .Select(r => r.GeoJson)
                .ToListAsync();

            // Combine all features into a FeatureCollection
            var featureCollection = new
            {
                type = "FeatureCollection",
                features = reports.Select(f => JObject.Parse(f))
            };

            // Return as JSON
            return Ok(featureCollection);
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
*/
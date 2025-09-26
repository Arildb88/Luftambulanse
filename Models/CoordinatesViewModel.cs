
// Einar

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Gruppe4NLA.Models
{
    // Represents a single coordinate
    public class CoordinatesViewModel
    {
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }
    }

    // Wrapper to hold new coordinate and submitted list
    public class CoordinatesViewModelWrapper
    {
        public CoordinatesViewModel NewCoordinate { get; set; } = new CoordinatesViewModel();
        public List<CoordinatesViewModel> SubmittedCoordinates { get; set; } = new List<CoordinatesViewModel>();
    }
}

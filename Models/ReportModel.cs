using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Gruppe4NLA.Models
{
    // Holds all information the users send in as a report
    public class ReportModel
    {
        public int Id { get; set; }

        // Some values are nullable using "?"
        public string? SenderName { get; set; }

        public string? DangerType { get; set; }

        public DateTime DateSent { get; set; }

        public string? Details { get; set; }

        // Coordinates are needed
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180")]
        public double? Longitude { get; set; }

        
    
    }

    // Wrapper to hold new coordinate and submitted list

    public class ReportModelWrapper
    {
        public ReportModel NewCoordinate { get; set; } = new ReportModel();
        public List<ReportModel> SubmittedCoordinates { get; set; } = new List<ReportModel>();
    }
}
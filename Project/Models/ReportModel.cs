using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Gruppe4NLA.Models
{
    // Holds all information the users send in as a report
    public class ReportModel
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        // Some values are nullable using "?"
        [Required(ErrorMessage = "Sendername is required")]
        public string? SenderName { get; set; }

        public string? DangerType { get; set; }

        public DateTime DateSent { get; set; }

        public string? Details { get; set; }

        [Required(ErrorMessage = "Height in meters is required")]
        [Range(0, 500, ErrorMessage = "Height in meters must range between 0 and 500" )]
        public double? HeightInnMeters { get; set; }
        
        public bool AreLighted { get; set; } = false;

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
        public ReportModel NewReport { get; set; } = new ReportModel();
        public List<ReportModel> SubmittedReport { get; set; } = new List<ReportModel>();
    }
}
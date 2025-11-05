using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace Gruppe4NLA.Models
{
    // Holds all information the users send in as a report

    public enum ReportStatus
    {
        Draft = 0,
        Submitted = 1

        // Further statuses can be added here
    }



    public class ReportModel
    {

        public int Id { get; set; }
        public string? UserId { get; set; }
        //public string? CaseworkerGroupId { get; set; }

        [Required(ErrorMessage = "Sendername is required")]
        public string? SenderName { get; set; }
        [Required(ErrorMessage = "Select an obstacle type")]

        // DangerType enum to make buttons easier to manage
        public DangerType? Type { get; set; }
        [MaxLength(100)]
        // Only needed if Other is selected
        public string? OtherDangerType { get; set; } 

        public DateTime DateSent { get; set; }

        public string? Details { get; set; }

        public string? GeoJson { get; set; }

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

        // Enum for different danger types, each with a specific integer value
        public enum DangerType
        {
            PowerLine = 1,
            Pole = 2,
            Construction = 3,
            Other = 99
        }

        public ReportStatus Status { get; set; } = ReportStatus.Draft;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? SubmittedAt { get; set; }


    }

    // Wrapper to hold new coordinate and submitted list

    public class ReportModelWrapper
    {
        public ReportModel NewReport { get; set; } = new ReportModel();
        public List<ReportModel> SubmittedReport { get; set; } = new List<ReportModel>();
    }
}
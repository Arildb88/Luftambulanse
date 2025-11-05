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

        // Keep legacy/display string
        public string? DangerType { get; set; }

        // Enum-backed selection used in views/controllers via "Type"
        public DangerTypeEnum Type { get; set; } = DangerTypeEnum.Cable;

        // Free-text when Type == Other
        public string? OtherDangerType { get; set; }

        public DateTime DateSent { get; set; }

        public string? Details { get; set; }

       
        [Range(0, 500, ErrorMessage = "Height in meters must range between 0 and 500")]
        public double? HeightInMeters { get; set; }

        public bool AreLighted { get; set; } = false;

        // Coordinates are needed
        [Required(ErrorMessage = "Latitude is required")]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90")]
        public double? Latitude { get; set; }

        [Required(ErrorMessage = "Longitude is required")]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 og 180")]
        public double? Longitude { get; set; }
    
        // GeoJSON string for geometry storage
        public string? GeoJson { get; set; }
        
        //Who the report is assigned to 
        public string? AssignedToUserId { get; set; }

        //Who performed the assignment
        public string? AssignedByUserId { get; set; }

        //Whenzthe assignment happened
        public DateTime? AssignedAtUtc { get; set; }

        //Workflow status
        public ReportStatus Status { get; set; } = ReportStatus.Submitted;

        //Last update timestamp
        public DateTime? UpdatedAtUtc { get; set; }

        // Rename enum to avoid clash with the string property
        public enum DangerTypeEnum
        {
            Cable = 0,
            Pole = 1,
            Construction = 2,
            Other = 99
        }
    }

    // Wrapper to hold new coordinate and submitted list
    public class ReportModelWrapper
    {
        public ReportModel NewReport { get; set; } = new ReportModel();
        public List<ReportModel> SubmittedReport { get; set; } = new List<ReportModel>();
    }

    //status for reports
    public enum ReportStatus
    {
        Draft = 0,
        Submitted = 1,
        Assigned = 2,
        InReview = 3,
        Completed = 4,
        Rejected = 5
    }
}

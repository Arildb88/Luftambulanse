using Gruppe4NLA.Controllers;
using Gruppe4NLA.Models.Validation;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace Gruppe4NLA.Models
{
  
    public enum ReportStatusCase
    {
        Draft = 0,        
        Submitted = 1,    
        Assigned = 2,     
        InReview = 3,     
        Completed = 4,    
        Rejected = 5      
    }

    // Main model representing an aviation obstacle report submitted by users
    // Contains location data, obstacle details, and workflow tracking information
    public class ReportModel
    {
        public int Id { get; set; }

        public string? UserId { get; set; }

        [Required(ErrorMessage = "Sendername is required")]
        public string? SenderName { get; set; }

        // Category of obstacle (Cable, Pole, Construction, Other)
        [Required(ErrorMessage = "You need to select an ObstacleType")]
        public DangerTypeEnum? Type { get; set; }

        public DateTime DateSent { get; set; }

        public string? Details { get; set; }

        [HeightRange]
        public double? HeightInMeters { get; set; }

        // Unit preference for display purposes only (not saved to database)
        [NotMapped]
        public string HeightUnit { get; set; } = "meters"; // "meters" or "feet"

        public bool AreLighted { get; set; } = false;

        public string? GeoJson { get; set; }

        public string? AssignedToUserId { get; set; }

        public DateTime? AssignedAtUtc { get; set; }

        public ReportStatusCase StatusCase { get; set; } = ReportStatusCase.Draft;

        public string? RejectReportReason { get; set; }

        public DateTime? UpdatedAtUtc { get; set; }

        // Defines the types of obstacles that can be reported
        public enum DangerTypeEnum
        {
            Cable = 0,         
            Pole = 1,          
            Construction = 2,  
            Other = 99         
        }

        public DateTime? SubmittedAt { get; set; }
    }

    // Container model for the report form view
    // Combines a new report being created with a list of previously submitted reports
    public class ReportModelWrapper
    {
        public ReportModel NewReport { get; set; } = new ReportModel();
        
        public List<ReportModel> SubmittedReport { get; set; } = new List<ReportModel>();
    }
}